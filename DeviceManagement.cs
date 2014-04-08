using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Ricoh.Extensions;
using Ricoh.Models;
using Ricoh.ricoh.deviceManagement;

namespace Ricoh
{
  public delegate void DeviceManagementEventHandler<T, U>(T sender, U eventArgs);

  /// <summary>
  /// Allows for a Ricoh device to be controlled or queried over the network.
  /// </summary>
  public class DeviceManagement : RicohEmbeddedSoapService, IDeviceManagement
  {
    private const uint MAX_RETRIES = 3;

    private readonly object _mutex = new Object();
    private DeviceAccessControl? _accessControl = null;

    private deviceManagementPortType _client;
    internal const string USAGE_CONTROL_DEVICE = "usageControl.applRestrict";
    internal const string USAGE_CONTROL_USER = "usageControl.userRestrict";
    internal const string USAGE_COUNTER_USER = "usageCounter.userCounter";

    /// <summary>Occurs when the user counter information has been retrieved.</summary>
    public event DeviceManagementEventHandler<DeviceManagement, UserCounter> UserCounterRetrieved;

    /// <summary>Occurs when the user counter information has been reset. This event is not fired when ClearCounters() is called.</summary>
    public event DeviceManagementEventHandler<DeviceManagement, UserCounter> UserCounterReset;

    /// <summary>Occurs when the user access control (ACL) information has been retrieved.</summary>
    public event DeviceManagementEventHandler<DeviceManagement, UserAccessControl> UserAccessControlRetrieved;

    /// <summary>Occurs when the user access control (ACL) information has been change.</summary>
    public event DeviceManagementEventHandler<DeviceManagement, UserAccessControl> UserAccessControlChanged;

    /// <summary>The proxy service client.</summary>
    internal deviceManagementPortType Client
    {
      get { return (_client ?? (_client = (deviceManagementPortType) GetClient())); }
      set { _client = value; }
    }

    /// <param name="hostname">The hostname or ip address of the copier to connect to.</param>
    /// <param name="username">The username for authentication. Default: "admin".</param>
    /// <param name="password">The password for authentiation.</param>
    public DeviceManagement(string hostname, string username = "admin", string password = null): base(hostname, username, password)
    {
    }

    /// <summary>
    /// GetValue's the endpoint address for the connection.
    /// </summary>
    /// <returns>The configured endpoint address.</returns>
    protected override EndpointAddress GetEndpointAddress()
    {
      return (new EndpointAddress(String.Format("http://{0}/DH/devicemanagement", Hostname)));
    }

    /// <summary>
    /// Creates and configured the proxy client.
    /// </summary>
    /// <returns>The service proxy / channel.</returns>
    private deviceManagementPortType GetClient()
    {
      Binding binding = GetBinding();
      EndpointAddress address = GetEndpointAddress();

      ChannelFactory<deviceManagementPortType> factory = new ChannelFactory<deviceManagementPortType>(binding, address);
      foreach (IEndpointBehavior behavior in GetEndpointBehaviors()) {
        factory.Endpoint.Behaviors.Add(behavior);
      }

      return (factory.CreateChannel());  
    }

    /// <summary>
    /// Get the capabilities for the services (limits, maximums, etc.)
    /// </summary>
    private void GetServiceCapability()
    {
      try {
        getServiceCapabilityResponse response = Client.getServiceCapability(new getServiceCapabilityRequest() { sessionId = SessionId });
        if (response != null) {
          Trace.TraceInformation("Retrieved deviceManagement service capabilities at {0}. Session: {1}", Hostname, SessionId);

          // We're not doing anything with the data here yet.
        }
      }
      catch (Exception e) {
        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "GetServiceCapability", e));
      }
    }

    /// <summary>
    /// A wrapper class to automatically connect and disconnect when executing the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    private T Connect<T>(Func<T> action)
    {
      if (action == null) return(default(T));

      // Check to make sure we are connected, if not implicitly connect.
      bool connectionEstablished = false;
      if (!IsConnected) {
        connectionEstablished = true;
        Connect();
      }

      // Invoke our external action.
      T result = action.Invoke();

      // Did we connect automatically on behalf of the user? If so, disconnect.
      if (connectionEstablished) Disconnect();

      return (result);
    }

    /// <summary>
    /// Connects and authenticates to the external service.
    /// </summary>
    /// <param name="timeLimit">The amount of time that the session is to be valid on the external service. If not specified, uses the configured value.</param>
    /// <returns>The session id to use for subsequent requests to the service.</returns>
    public string Connect(ushort? timeLimit = null)
    {
      if (IsConnected) Disconnect();
      
      try {
        startSessionResponse response = Client.startSession(new startSessionRequest() {
          stringIn = GetAuthenticationScheme(), 
          timeLimit = timeLimit ?? TimeLimit
        });
        if (IsOK(response.returnValue)) {
          SessionId = response.stringOut;
          Trace.TraceInformation("Connected to DeviceManagement at {0}. Session: {1}", Hostname, SessionId);

          // Grab the service capabilities for the device
          GetServiceCapability();
        }
      }
      catch (Exception e) {
        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "Connect", e));
      }
      return (SessionId);
    }

    /// <summary>
    /// Get's the object capabilities for the object. These are the supported fields and supported
    /// ranges for the values.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="objectId">The id of the object to retrieve.</param>
    /// <returns>The objectCapability of the specified id if found; otherwise returns null.</returns>
    private objectCapability GetObjectCapability(uint deviceId, string objectId)
    {
      uint id;
      if (UInt32.TryParse(objectId, out id)) {
        return (GetObjectCapability(deviceId, id));
      }

      Trace.TraceWarning("Specified objectId was invalid: {0}.", objectId);
      return (null);
    }

    /// <summary>
    /// Get's the object capabilities for the object. These are the supported fields and supported
    /// ranges for the values.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="objectId">The id of the object to retrieve.</param>
    /// <returns>The objectCapability of the specified id if found; otherwise returns null.</returns>
    private objectCapability GetObjectCapability(uint deviceId, uint objectId)
    {
      Trace.TraceInformation("Retrieving object capabilities for {0}.", objectId);
      return (Client.getObjectCapability(SessionId, deviceId, objectId, "ALL"));
    }

    /// <summary>
    /// The actual API queries the capabilities for each and every object which is
    /// very slow. Since the object properties should be mostly the same (???), 
    /// we're only going to query the capabilities for the first item and then reuse it.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="objectIds">The object ids that were retrieved.</param>
    /// <returns>The first valid objectCapability that will be used as the default.</returns>
    private objectCapability GetDefaultObjectCapability(uint deviceId, string[] objectIds)
    {
      if ((objectIds == null) || (objectIds.Length == 0)) return (null);

      objectCapability capability = null;
      if (objectIds.Any(objectId => (capability = GetObjectCapability(deviceId, objectId)) != null)) {
        return (capability);
      }
      return (null);
    }

    /// <summary>
    /// Retrieves the user counter data for the specified object id.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="objectId">The objectId to retrieve.</param>
    /// <param name="capability">The capabilities of the object to query.</param>
    /// <param name="retry">Internal parameter used to keep track of the number of retries.</param>
    /// <returns>The user counter for the specified objectId.</returns>
    private UserCounter GetUserCounter(uint deviceId, uint objectId, ref objectCapability capability, uint retry = 0)
    {
      // The actual API queries the capabilities for each and every object which is
      // very slow. Since the object properties should be mostly the same, we're only 
      // going to query the capabilities for the first item and then reuse it.
      // query the capabilities for the first item and then reuse them.
      lock(_mutex) {
        if (capability == null) {
          Trace.TraceInformation("Retrieving object capabilities for {0}.", objectId);
          capability = GetObjectCapability(deviceId, objectId);
        }
      }

      string[] fields = capability.fieldList.Select(f => f.name).ToArray();
      Trace.TraceInformation("Retreiving object {0} attributes: {1}.", objectId, String.Join(",", fields));
      try {
        var @object = Client.getObject(SessionId, deviceId, objectId, fields);
        if (@object != null) {
          Trace.TraceInformation("Data received for object {0}: {1}", objectId, 
                                 String.Join(",", @object.fieldList.Select(f => String.Format("{0}[{1}]={2}", f.name, f.type, f.value))));
          return (new UserCounter(@object.oid, @object.fieldList, capability != null ? capability.fieldList : null));
        }
      }
      catch (Exception e) {
        // Device is too slow... not responding. Requeue the request.
        if ((e is EntryPointNotFoundException) || (e is CommunicationException)) {
          if (retry < MAX_RETRIES) {
            Trace.TraceWarning("Connection timed out for: {0}. Retry: {1}", Hostname, retry + 1);
            return (GetUserCounter(deviceId, objectId, ref capability, retry + 1));
          }
        }

        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "GetUserCounter", e));
      }
      return (null);
    }

    /// <summary>
    /// Updates the specified user counter fields for the specified object
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="name">The name or index of the object.</param>
    /// <param name="oid">The unique id associated with the object.</param>
    /// <param name="class">The class / object type being updated.</param>
    /// <param name="fields">The fields being updated.</param>
    /// <param name="replaceAll">Indicates if all of the values should be replaced.</param>
    /// <returns>True if the counters were reset, false if otherwise.</returns>
    protected virtual bool Update(uint deviceId, string name, uint oid, field[] fields, string @class, bool replaceAll = false)
    {
      return (Update(deviceId, name, oid, fields, @class, new [] { new property() { propName = "replaceAll", propVal = replaceAll ? "true" : "false" } }));
    }

    /// <summary>
    /// Updates the specified user counter fields for the specified object
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="name">The name or index of the object.</param>
    /// <param name="oid">The unique id associated with the object.</param>
    /// <param name="class">The class / object type being updated.</param>
    /// <param name="fields">The fields being updated.</param>
    /// <param name="options">The options for the update.</param>
    /// <param name="retry">Internal parameter used to keep track of the number of retries.</param>
    /// <returns>True if the counters were reset, false if otherwise.</returns>
    protected virtual bool Update(uint deviceId, string name, uint oid, field[] fields, string @class, property[] options = null, uint retry = 0)
    {
      if (String.IsNullOrEmpty(name)) return (false);
      if (options == null) options = new[] { new property() { propName = "replaceAll", propVal = "false" } };

      var obj = new @object() {
        name = name,
        @class = @class,
        oid = oid,
        fieldList = fields
      };

      try {
        updateObjectResponse response = Client.updateObject(new updateObjectRequest(SessionId, deviceId, obj, options));
        if ((response != null) && (IsOK(response.returnValue))) {
          Trace.TraceInformation("Update object for object {0} succeeded [{1}]. Counters: {2}", name, response.returnValue,
                                 String.Join(",", fields.Select(f => f.name)));
          return (true);
        }

        Trace.TraceError("Update object for object {0} failed [{1}]. Counters: {2}", name,
                         response != null ? response.returnValue : "NULL_ERROR",
                         String.Join(",", fields.Select(f => f.name)));
      }
      catch (Exception e) {
        // Device is too slow... not responding. Requeue the request.
        if ((e is EntryPointNotFoundException) || (e is CommunicationException)) {
          if (retry < MAX_RETRIES) {
            Trace.TraceWarning("Connection timed out for: {0}. Retry: {1}", Hostname, retry + 1);
            return (Update(deviceId, name, oid, fields, @class, options,  retry + 1));
          }
        }

        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "Update", e));
      }
      return (false);
    }

    /// <summary>
    /// Checks to see if the specified field is updatable.
    /// </summary>
    /// <param name="field">The field to inspect.</param>
    /// <returns>True if updatable, false if otherwise.</returns>
    private bool IsUserCounterUpdatable(IDeviceField field)
    {
      if (field == null) return(false);

      // The *Account,*Total files indicate they are writable, but older firmware will throw a MANAGEMENT_BAD_OBJECT_ID.
      // These fields are NOT writable even though the capability may indicate otherwise.
      return (field.Type.IsNumeric() &&
             !(field.Name ?? String.Empty).EndsWith("Account", StringComparison.CurrentCultureIgnoreCase) &&
             !(field.Name ?? String.Empty).EndsWith("Total", StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Checks to see if the specified field capability is updatable.
    /// </summary>
    /// <param name="capability">The field to inspect.</param>
    /// <returns>True if updatable, false if otherwise.</returns>
    private bool IsUserCounterCapabilityUpdatable(fieldCapability capability)
    {
      if (capability == null) return (false);

      // The *Account,*Total files indicate they are writable, but older firmware will throw a MANAGEMENT_BAD_OBJECT_ID.
      // These fields are NOT writable even though the capability may indicate otherwise.
      return (capability.IsNumber() &&
             !(capability.name ?? String.Empty).EndsWith("Account", StringComparison.CurrentCultureIgnoreCase) &&
             !(capability.name ?? String.Empty).EndsWith("Total", StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Clears all of the counters for the specified user to 0.
    /// </summary>
    /// <param name="counter">The counter to reset the values for.</param>
    /// <param name="deviceId">The internal id of the device to clear.</param>
    /// <returns>True if the counters were reset, false if otherwise.</returns>
    public bool Clear(UserCounter counter, uint deviceId = 0)
    {
      if (counter == null) return (false);
      return(Connect(() => {
        IEnumerable<field> fields = counter.Fields.GetUpdatableFields(IsUserCounterUpdatable).Clear();
        if ((fields == null) || (!fields.Any())) return (false);

        return (Update(deviceId, Convert.ToString(counter.Index), counter.Id, fields.ToArray(), USAGE_COUNTER_USER, true));
      }));
    }

    /// <summary>
    /// Resets all of the counters for the specified users to 0.
    /// </summary>
    /// <param name="counters">The counters to reset the values for.</param>
    /// <param name="deviceId">The internal id of the device to clear.</param>
    /// <returns>The number of counters that were reset.</returns>
    public int Clear(IEnumerable<UserCounter> counters, uint deviceId = 0)
    {
      if ((counters == null) || (!counters.Any())) return (0);

      return(Connect(() => {
        int count = 0;
        counters.AsParallel().ForAll(c => {
          count += (Clear(c, deviceId) ? 1 : 0);
        });
        return(count);
      }));
    }

    /// <summary>
    /// Clears all of the counters for the system to zero.
    /// </summary>
    /// <param name="deviceId">The internal id of the device to clear.</param>
    /// <remarks>
    /// For performance, if you previously called <see cref="GetUserCounters"/>, instead of 
    /// calling this function, you should instead call <see cref="Clear(System.Collections.Generic.IEnumerable{Ricoh.Models.UserCounter})"/> passing
    /// in the results from the previous call. In order to avoid some of the overhead of
    /// retrieving each object, only the ids are sent and the default object capability is
    /// used to clear the counters for each user.
    /// </remarks>
    public void Clear(uint deviceId = 0)
    {
      Connect(() => {
        Trace.TraceInformation("Retrieving all objects for {0}", USAGE_COUNTER_USER);
        try {
          string[] objectIds = Client.getObjects(SessionId, deviceId, USAGE_COUNTER_USER);
          if ((objectIds != null) && (objectIds.Length > 0)) {
            // Get the "default" object capabilities
            // -------------------------------------------------------
            // Instead of querying each object, we're just going to use the fields returned
            // for the first item for each of the objectIds that were retrieved. To do this
            // "correctly" (i.e. slowly), we should query each object capabilities, then do
            // the update. Once we have the fields, we just get back those that are updateable
            objectCapability capability = GetDefaultObjectCapability(deviceId, objectIds);
            field[] fields = capability.fieldList.GetUpdatableFields(IsUserCounterCapabilityUpdatable);

            objectIds.AsParallel().ForAll((i) => {
              uint objectId;
              if (UInt32.TryParse(i, out objectId)) {
                if (Update(deviceId, Convert.ToString(UserCounter.ExtractIndexFromObjectId(objectId)), objectId, fields, USAGE_COUNTER_USER, true)) {
                  Trace.TraceInformation("Counters reset for object {0}. Counters: {1}", objectId,
                                         String.Join(",", fields.Select(f => f.name)));
                }
                else {
                  Trace.TraceError("Reset counter for object {0} failed.", objectId,
                                   String.Join(",", fields.Select(f => f.name)));
                }
              }
            });
          }
        }
        catch (Exception e) {
          Trace.TraceError(e.Message);
          Trace.TraceError(e.StackTrace);

          throw (new RicohOperationFailedException(this, "Clear", e));
        }

        return (true); // Required to satisfy Func<T>
      });
    }

    /// <summary>
    /// Get's the access control capabilities / restrictions for the specified device.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    public DeviceAccessControl GetDeviceAccessControlCapabilities(uint deviceId = 0)
    {
      // Make sure we haven't already processed the restrictions.
      if (_accessControl != null) return (_accessControl.GetValueOrDefault(DeviceAccessControl.None));

      return (Connect(() => {
        _accessControl = DeviceAccessControl.None;

        string[] objectIds = Client.getObjects(SessionId, deviceId, USAGE_CONTROL_DEVICE);
        if ((objectIds != null) && (objectIds.Length > 0)) {
          Trace.TraceInformation("Retrieved the following objectIds for {0}", USAGE_CONTROL_DEVICE);

          // For each of the object ids (corresponding to copy, fax, printer, scanner, etc.), 
          // get the capabilities for each.
          objectIds.AsParallel().ForAll((objectId) => {
            objectCapability capability = GetObjectCapability(deviceId, objectId);
            if (capability != null) {
              DeviceAccessControl deviceAccessControl = capability.ToDeviceAccessControl();
              Trace.TraceInformation("Device supports {0} restrictions (ACLs).", deviceAccessControl);

              _accessControl |= deviceAccessControl;
            }
          });
        }
        return (_accessControl.GetValueOrDefault(DeviceAccessControl.None));
      }));
    }

    #region Access Controls
    /// <summary>
    /// Checks to see if the device supports fax access controls.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the device supports fax access controls.</returns>
    public bool HasFaxAccessControl(uint deviceId = 0)
    {
      return (GetDeviceAccessControlCapabilities(deviceId).HasFaxAccessControl());
    }

    /// <summary>
    /// Checks to see if the device supports document server access controls.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the device supports document server access controls.</returns>
    public bool HasDocumentServerAccessControl(uint deviceId = 0)
    {
      return (GetDeviceAccessControlCapabilities(deviceId).HasDocumentServerAccessControl());
    }

    /// <summary>
    /// Checks to see if the device supports printer access controls.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the device supports printer access controls.</returns>
    public bool HasPrinterAccessControl(uint deviceId = 0)
    {
      return (GetDeviceAccessControlCapabilities(deviceId).HasPrinterAccessControl());
    }

    /// <summary>
    /// Checks to see if the device supports copier access controls.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the device supports copier access controls.</returns>
    public bool HasCopierAccessControl(uint deviceId = 0)
    {
      return (GetDeviceAccessControlCapabilities(deviceId).HasCopierAccessControl());
    }

    /// <summary>
    /// Checks to see if the device supports scanner access controls.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the device supports scanner access controls.</returns>
    public bool HasScannerAccessControl(uint deviceId = 0)
    {
      return (GetDeviceAccessControlCapabilities(deviceId).HasScannerAccessControl());
    }
    #endregion

    /// <summary>
    /// Retrieves the user restrictions / permission for the specified user.
    /// </summary>
    /// <param name="objectId">The id of the object to retrieve.</param>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <param name="capability">The default capabilities supported by the object.</param>
    /// <param name="retry">Internal parameter used to keep track of the number of retries.</param>
    /// <returns>The <see cref="UserAccessControl"/> object for the user.</returns>
    private UserAccessControl GetUserAccessControl(uint objectId, uint deviceId, ref objectCapability capability, uint retry = 0)
    {
      // The actual API queries the capabilities for each and every object which is
      // very slow. Since the object properties should be mostly the same, we're only 
      // going to query the capabilities for the first item and then reuse it.
      // query the capabilities for the first item and then reuse them.
      if (capability == null) {
        Trace.TraceInformation("Retrieving object capabilities for {0}.", objectId);
        capability = GetObjectCapability(deviceId, objectId);
      }

      string[] fields = capability.fieldList.Select(f => f.name).ToArray();
      Trace.TraceInformation("Retreiving object {0} attributes: {1}.", objectId, String.Join(",", fields));
      try {
        var @object = Client.getObject(SessionId, deviceId, objectId, fields);
        if (@object != null) {
          Trace.TraceInformation("Data received for object {0}: {1}", objectId,
                                 String.Join(",", @object.fieldList.Select(f => String.Format("{0}[{1}]={2}", f.name, f.type, f.value))));
          return (new UserAccessControl(@object.oid, @object.fieldList, capability != null ? capability.fieldList : null));
        }
      } catch (Exception e) {
        // Device is too slow... not responding. Requeue the request.
        if ((e is EntryPointNotFoundException) || (e is CommunicationException)) {
          if (retry < MAX_RETRIES) {
            Trace.TraceWarning("Connection timed out for: {0}. Retry: {1}", Hostname, retry + 1);
            return (GetUserAccessControl(objectId, deviceId, ref capability, retry + 1));
          }
        }

        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "GetUserAccessControl", e));
      }
      return (null);
    }

    /// <summary>
    /// Retrieves all the current user restrictions / permissions from the system.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>The collection of user counters for the device.</returns>
    public IEnumerable<UserAccessControl> GetUserAccessControls(uint deviceId = 0)
    {
      IList<UserAccessControl> userAccessControls = new List<UserAccessControl>();
      return (Connect(() => {
        Trace.TraceInformation("Retrieving all objects for {0}", USAGE_CONTROL_USER);
        try {
          string[] objectIds = Client.getObjects(SessionId, deviceId, USAGE_CONTROL_USER);
          if ((objectIds != null) && (objectIds.Length > 0)) {
            Trace.TraceInformation("{0} objects: {1}", USAGE_CONTROL_USER, String.Join(",", objectIds));

            // Get the "default" object capabilities
            objectCapability capability = GetDefaultObjectCapability(deviceId, objectIds);

            // In parallel, retrieve the access controls for the user.
            objectIds.AsParallel().ForAll((i) => {
              uint objectId;
              if (UInt32.TryParse(i, out objectId)) {
                UserAccessControl accessControl = GetUserAccessControl(objectId, deviceId, ref capability);
                if (accessControl != null) {
                  OnUserAccessControlRetrieved(accessControl);
                  userAccessControls.Add(accessControl);
                }
              }
            });
          }
          return (userAccessControls);
        } catch (Exception e) {
          Trace.TraceError(e.Message);
          Trace.TraceError(e.StackTrace);

          throw (new RicohOperationFailedException(this, "GetUserRestrictions", e));
        }
      }));
    }

    /// <summary>
    /// Restricts access to the fax functionality for the device.
    /// </summary>
    /// <param name="deviceId">The internal id of the device. If null, then ignore or don't modify attribute.</param>
    /// <param name="currentAccessControl">The existing <see cref="UserAccessControl"/> settings.</param>
    /// <param name="accessControl">The device controls to modify.</param>
    /// <param name="allow">Enable or disable access. If null, then ignore or don't modify attribute.</param>
    /// <returns>True if the call was successful, false if otherwise.</returns>
    protected bool SetAccessControl(uint deviceId, UserAccessControl currentAccessControl, DeviceAccessControl accessControl, bool allow = false)
    {
      return (Connect(() => {
        try {

          IEnumerable<field> fields = currentAccessControl.Fields.GetUpdatableFields(f => f.Type == typeof(bool));
          if ((fields == null) || (!fields.Any())) return (false);

          IEnumerable<string> names = accessControl.GetExternalNames(); // Get all of the external names that match the requested accessControl type.
          
          // Update the requested changes.
          fields.AsParallel().ForAll(f => {
            if (names.Any(n => String.Equals(n, f.name, StringComparison.CurrentCultureIgnoreCase))) {
              // The external service views the ON / OFF from a RESTRICTION point of view instead of ALLOW
              bool toogle = !allow;// f.IsFax() ? ! allow :allow;

              Trace.TraceWarning("Changing accessControl '{1}' from '{2}' to '{3}' for {0}.", currentAccessControl.Id, f.name, f.value, toogle.ToOnOff());
              f.value = toogle.ToOnOff(); 
            }
          }); 

          return (Update(deviceId, 
                         Convert.ToString(currentAccessControl.Index), currentAccessControl.Id, 
                         fields.ToArray(), USAGE_CONTROL_USER, false));
        } catch (Exception e) {
          Trace.TraceError(e.Message);
          Trace.TraceError(e.StackTrace);

          throw (new RicohOperationFailedException(this, "Restrict", e));
        }
      }));
    }
    
    /// <summary>
    /// Restricts access to the specified device functions for the device.
    /// </summary>
    /// <param name="currentAccessControl">The existing <see cref="UserAccessControl"/> settings.</param>
    /// <param name="accessControl">The device controls to modify.</param>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the call was successful, false if otherwise.</returns>
    public bool RestrictAccess(UserAccessControl currentAccessControl, DeviceAccessControl accessControl, uint deviceId = 0)
    {
      return (SetAccessControl(deviceId, currentAccessControl, accessControl, false));
    }

    /// <summary>
    /// Allows access to the specified device functions for the device.
    /// </summary>
    /// <param name="currentAccessControl">The existing <see cref="UserAccessControl"/> settings.</param>
    /// <param name="accessControl">The device controls to modify.</param>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>True if the call was successful, false if otherwise.</returns>
    public bool AllowAccess(UserAccessControl currentAccessControl, DeviceAccessControl accessControl, uint deviceId = 0)
    {
      return (SetAccessControl(deviceId, currentAccessControl, accessControl, true));
    }

    /// <summary>
    /// Retrieves the current user counters from the system.
    /// </summary>
    /// <param name="deviceId">The internal id of the device.</param>
    /// <returns>The collection of user counters for the device.</returns>
    public IEnumerable<UserCounter> GetUserCounters(uint deviceId = 0)
    {
      return(Connect(() => {
        IList<UserCounter> counters = new List<UserCounter>();

        Trace.TraceInformation("Retrieving all objects for {0}", USAGE_COUNTER_USER);
        try {
          string[] objectIds = Client.getObjects(SessionId, deviceId, USAGE_COUNTER_USER);
          if ((objectIds != null) && (objectIds.Length > 0)) {
            Trace.TraceInformation("{0} objects: {1}", USAGE_COUNTER_USER, String.Join(",", objectIds));

            // Get the "default" object capabilities
            objectCapability capability = GetDefaultObjectCapability(deviceId, objectIds);

            // In parallel, retrieve the user counters.
            objectIds.AsParallel().ForAll((i) => {
              uint objectId;
              if (UInt32.TryParse(i, out objectId)) {
                UserCounter counter = GetUserCounter(deviceId, objectId, ref capability);
                if (counter != null) {
                  OnUserCounterRetrieved(counter);
                  counters.Add(counter);
                }
                else {
                  Trace.TraceWarning("Failed to retrieve information for object {0}.", objectId);
                }
              }
            });
          }
        }
        catch (Exception e) {
          Trace.TraceError(e.Message);
          Trace.TraceError(e.StackTrace);

          throw (new RicohOperationFailedException(this, "GetUserCounters", e));
        }
        return (counters);
      }));
    }

    /// <summary>
    /// Disconnects / releases the session key for the external service.
    /// </summary>
    /// <returns>True if the session release was acknowledged by the external service, false if otherwise.</returns>
    public bool Disconnect()
    {
      if (! IsConnected) return (false);

      try {
        var response = Client.terminateSession(SessionId);
        if (IsOK(response)) {
          Trace.TraceInformation("Disconnected from uDirectory at {0}. Session: {1}", Hostname, SessionId);

          SessionId = null;
          return (true);
        }
      }
      catch (Exception e) {
        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "Disconnect", e));
      }
      return (false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public override void Dispose()
    {
      if (IsConnected) {
        Disconnect();
      }
    }

    /// <summary>
    /// Occurs when user counter has been reset.
    /// </summary>
    /// <param name="counter">The user counter data that was reset.</param>
    protected virtual void OnUserCounterReset(UserCounter counter)
    {
      if (UserCounterReset != null) {
        UserCounterReset(this, counter);
      }
    }

    /// <summary>
    /// Occurs when user counter data has be retrieved.
    /// </summary>
    /// <param name="counter">The user counter data that was retrieved.</param>
    protected virtual void OnUserCounterRetrieved(UserCounter counter)
    {
      if (UserCounterRetrieved != null) {
        UserCounterRetrieved(this, counter);
      }
    }

    /// <summary>
    /// Occurs when user access control (ACL) has be retrieved.
    /// </summary>
    /// <param name="control">The user access control data that was retrieved.</param>
    protected virtual void OnUserAccessControlRetrieved(UserAccessControl control)
    {
      if (UserAccessControlRetrieved != null) {
        UserAccessControlRetrieved(this, control);
      }
    }

    /// <summary>
    /// Occurs when user access control (ACL) has been changed.
    /// </summary>
    /// <param name="control">The new user access control data.</param>
    protected virtual void OnUserAccessControlChanged(UserAccessControl control)
    {
      if (UserAccessControlChanged != null) {
        UserAccessControlChanged(this, control);
      }
    }
  }
}
