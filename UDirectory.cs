using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Ricoh.Models;
using Ricoh.ricoh.uDirectory;
using getServiceCapabilityRequest = Ricoh.ricoh.uDirectory.getServiceCapabilityRequest;
using getServiceCapabilityResponse = Ricoh.ricoh.uDirectory.getServiceCapabilityResponse;
using property = Ricoh.ricoh.uDirectory.property;
using startSessionRequest = Ricoh.ricoh.uDirectory.startSessionRequest;
using startSessionResponse = Ricoh.ricoh.uDirectory.startSessionResponse;

namespace Ricoh
{
  public delegate void UDirectoryEventHandler<T, U>(T sender, U eventArgs);

  public class UDirectory: RicohEmbeddedSoapService, IUDirectory
  {
    private uDirectoryPortType _client;
    private RicohSessionType _ricohSessionType = RicohSessionType.SharedSession;

    /// <summary>The list of object classes that the device supports.</summary>
    private property[] _capabilities;
    /// <summary>The default (complete) list of supported fields / properties.</summary>
    private static readonly string[] DEFAULT_SUPPORTED_FIELDS = {
      "entryType", "id", "name", "longName",
      "phoneticName", "index", "isUser","isGroup","isBuiltInUser","isBuiltInGroup","builtIn","accessControlPolicy",
      "passwordEncoding", "isDestination", "isSender",
      "auth:", "auth:name", "auth:password", 
      "password:", "password:password","password:usedForMailSender", "password:usedForRemoteFolder", "password:passwordEncoding", 
      "mail:", "mail:address", "mail:parameter", "mail:isDirectSMTP", 
      "fax:", "fax:number", "fax:lineType", "fax:isAbroad", "fax:parameter", 
      "ipfax:","ipfax:address","ipfax:parameter","ipfax:type",
      "ifax:", "ifax:address", "ifax:parameter", "ifax:isDirectSMTP",
      "faxAux:", "faxAux:ttiNo", "faxAux:label1", "faxAux:label2String", "faxAux:messageNo", 
      "faxRelay:","faxRelay:numbers",
      "remoteFolder:","remoteFolder:type", "remoteFolder:serverName", "remoteFolder:path", "remoteFolder:accountName", 
        "remoteFolder:password", "remoteFolder:port", "remoteFolder:characterEncoding", "remoteFolder:passwordEncoding", 
        "remoteFolder:select", "remoteFolder:logonMode",
      "ldap:", "ldap:accountName", "ldap:password", "ldap:passwordEncoding", "ldap:select", 
      "smtp:", "smtp:accountName", "smtp:password", "smtp:passwordEncoding", "smtp:select",
      "tagId"
    };

    /// <summary>
    /// The list of recognized fields that the uDirectory service can return. 
    /// GetServiceCapability() will dynamically update this value
    /// </summary>
    private string[] SupportedFields
    {
      get
      {
        string[] result = DEFAULT_SUPPORTED_FIELDS;
        if (_capabilities == null) return (result);

        var property = _capabilities.Get("entryProperty");
        if (property.HasValue()) {
          result = property.propVal.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
        return (result);
      }
    }
    
    /// <summary>The maximum number of object properties that can be returned. GetServiceCapability() will dynamically update this value.</summary>
    private ushort MaxObjectPerCall
    {
      get
      {
        ushort result = 50;
        if (_capabilities == null) return(result);
        
        var property = _capabilities.Get("maxObjectPerCall");
        if (property.HasValue()) {
          if (UInt16.TryParse(property.propVal, out result)) {
          }
        }
        return(result);
      }
    }

    /// <summary>The maximum number of tags allowed per object.</summary>
    private ushort MaxTagsAllowed
    {
      get
      {
        ushort result = 1;
        if (_capabilities == null) return (result);

        var property = _capabilities.Get("maxTagPerEntry");
        if (property.HasValue()) {
          if (UInt16.TryParse(property.propVal, out result)) {
          }
        }
        return (result);
      }
    }

    /// <summary>The proxy service client.</summary>
    private uDirectoryPortType Client
    {
      get { return (_client ?? (_client = GetClient())); }
      set { _client = value; }
    }

    /// <summary>Indicates the type of session to establish (exclusive or shared).</summary>
    public RicohSessionType SessionType
    {
      get { return(_ricohSessionType); }
      set { _ricohSessionType = value; }
    }

    /// <summary>Occurs when the user has been added to the address book.</summary>
    public event UDirectoryEventHandler<UDirectory, AddressBookEntry> AddressBookEntryAdded;
    /// <summary>Occurs when the user has been removed from the address book.</summary>
    public event UDirectoryEventHandler<UDirectory, uint> AddressBookEntryRemoved;
    /// <summary>Occurs when the user counter information has been retrieved.</summary>
    public event UDirectoryEventHandler<UDirectory, AddressBookEntry> AddressBookEntryRetrieved;
    /// <summary>Occurs when an invalid address book entry is received.</summary>
    public event UDirectoryEventHandler<UDirectory, IDictionary> InvalidAddressBookEntry;
    
    /// <param name="hostname">The hostname or ip address of the copier to connect to.</param>
    /// <param name="username">The username for authentication. Default: "admin".</param>
    /// <param name="password">The password for authentiation.</param>
    public UDirectory(string hostname, string username = "admin", string password = null): base(hostname, username, password)
    {
    }

    /// <summary>
    /// GetValue's the endpoint address for the connection.
    /// </summary>
    /// <returns>The configured endpoint address.</returns>
    protected override EndpointAddress GetEndpointAddress()
    {
      return (new EndpointAddress(String.Format("http://{0}/DH/udirectory", Hostname)));
    }

    /// <summary>
    /// Creates and configured the proxy client.
    /// </summary>
    /// <returns>The service proxy / channel.</returns>
    private uDirectoryPortType GetClient()
    {
      Binding binding = GetBinding();
      EndpointAddress address = GetEndpointAddress();

      ChannelFactory<uDirectoryPortType> factory = new ChannelFactory<uDirectoryPortType>(binding, address);
      foreach(IEndpointBehavior behavior in GetEndpointBehaviors()) {
        factory.Endpoint.Behaviors.Add(behavior);
      }
      return (factory.CreateChannel());  
    }

    /// <summary>
    /// Get the capabilities for the services (limits, maximums, etc.)
    /// </summary>
    private void GetServiceCapability()
    {
      getServiceCapabilityResponse response = Client.getServiceCapability(new getServiceCapabilityRequest() { sessionId = SessionId });
      try {
        if (response != null) {
          Trace.TraceInformation("Retrieved uDirectory service capabilities at {0}. Session: {1}", Hostname, SessionId);
          _capabilities = response.returnValue;

          Trace.TraceInformation("  Supported fields:d {0}", String.Join(",", SupportedFields));
          Trace.TraceInformation("  Max objects per call: {0}", MaxObjectPerCall);
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
    /// <param name="sessionType">Indicates the type of connection to perform.</param>
    private T Connect<T>(Func<T> action, RicohSessionType sessionType = RicohSessionType.SharedSession)
    {
      if (action == null) return(default(T));

      // Are we connected in the right mode? If not, then disconnect to reestablish the connection
      if ((IsConnected) && (sessionType != SessionType)) Disconnect();

      // Check to make sure we are connected, if not implicitly connect.
      bool connectionEstablished = false;
      if (!IsConnected) {
        connectionEstablished = true;
        Connect(null, sessionType);
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
    /// <param name="sessionType">Indicates the type of session to connect with.</param>
    /// <returns>The session id to use for subsequent requests to the service.</returns>
    public string Connect(ushort? timeLimit = null, RicohSessionType? sessionType = null)
    {
      if (IsConnected) Disconnect();
      
      try {
        startSessionResponse response = Client.startSession(new startSessionRequest() {
          stringIn = GetAuthenticationScheme(),
          timeLimit = timeLimit ?? TimeLimit, 
          lockMode = (sessionType ?? SessionType).ToExternalServiceValue()
        });
        if (IsOK(response.returnValue)) {
          SessionId = response.stringOut;
          Trace.TraceInformation("Connected to uDirectory at {0}. Session: {1}", Hostname, SessionId);

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
    /// For the specified id, retrieves the detailed address book data.
    /// </summary>
    /// <param name="id">The id to retrieve.</param>
    /// <param name="selectProps">The properties to retrieve.</param>
    /// <returns>The parsed address book entry.</returns>
    private AddressBookEntry GetAddressBookEntry(uint id, string[] selectProps = null)
    {
      return(GetAddressBooksEntryForIds(new [] { id.ToString() }, selectProps).FirstOrDefault());
    }

    /// <summary>
    /// For the specified ids, retrieves the detailed address book data.
    /// </summary>
    /// <param name="ids">The ids to retrieve.</param>
    /// <param name="selectProps">The properties to retrieve.</param>
    /// <returns>The parsed address book entries.</returns>
    private IEnumerable<AddressBookEntry> GetAddressBooksEntryForIds(string[] ids, string[] selectProps = null)
    {
      if ((ids == null) || (ids.Length <= 0)) return (new AddressBookEntry[0]);

      return(Connect(() => {
        IList<AddressBookEntry> entries = new List<AddressBookEntry>();
        Trace.TraceInformation("Getting address book attributes for {0}", String.Join(",", ids));
        try {
          property[][] rowList = Client.getObjectsProps(SessionId, ids.Select(x => String.Format("entry:{0}", x)).ToArray(), selectProps ?? SupportedFields, null);
          if (rowList != null) {
            foreach (var row in rowList) {
              try {
                var entry = new AddressBookEntry(row);
                OnAddressBookEntryRetrieved(entry);
                entries.Add(entry);
              }
              catch (InvalidRicohAddressBookEntryException e) {
                Trace.TraceError("Failed parsing the address book entry: {0}",
                                 String.Join(",", e.Data.Keys.Cast<string[]>()
                                                             .Select(k => String.Format("{0}={1}", k, e.Data[k]))));
                OnInvalidAddressBookEntry(e.Data);
              }
            }
            return (entries);
          }
        }
        catch (Exception e) {
          Trace.TraceError(e.Message);
          Trace.TraceError(e.StackTrace);

          throw (new RicohOperationFailedException(this, "GetAddressBooksEntryForIds", e));
        }
        return (new AddressBookEntry[0]);
      }));
    }

    /// <summary>
    /// Performs a search of the client.
    /// </summary>
    /// <param name="rowOffset">Teh row number to start at when retrieve rows. Defaults to 0. </param>
    /// <param name="fromClass">The type of address book items to retrieve.</param>
    /// <param name="parser">The action to run to parse the results.</param>
    /// <param name="selectProps">The properties to retrieve.</param>
    private IEnumerable<T> Search<T>(Func<property[][], IEnumerable<T>> parser, string fromClass = "entry", string[] selectProps = null, uint rowOffset = 0, uint parentObjectId = 0)
    {
      try {
        var result = new List<T>();

        searchObjectsResponse response;
        do {
          response = Client.searchObjects(new searchObjectsRequest() {
            sessionId = SessionId,
            parentObjectId = parentObjectId,
            selectProps = selectProps ?? new[] { "id" },
            fromClass = fromClass,
            rowOffset = rowOffset,
            rowCount = MaxObjectPerCall
          });
          if ((response != null) && ((IsOK(response.returnValue)) || (response.returnValue == RicohResponseStatus.END_OF_DIRECTORY))) {
            Trace.TraceInformation("Retreived item entries {0}-{1}/{2}", rowOffset, (rowOffset + MaxObjectPerCall < response.numOfResults) ? rowOffset + MaxObjectPerCall - 1 : response.numOfResults, response.numOfResults);

            var set = parser.Invoke(response.rowList);
            if (set != null) result.AddRange(set);

            rowOffset += response.numOfResults;
          }
        } while ((response != null) && (IsOK(response.returnValue)));

        return (result);
      }
      catch (Exception e) {
        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "Search", e));
      }
    }

    /// <summary>
    /// Searches the address book, retrieving all of the IDs for the specified entry type.
    /// </summary>
    /// <param name="selectProps">The properties to retrieve.</param>
    private IEnumerable<AddressBookEntry> SearchDirectory(string[] selectProps = null)
    {
      try {
        return (Search((properties) => {
          // Process the multi-dimensional array extracting just the id field (only value returned)
          IEnumerable<string> ids = properties.SelectMany(x => x)
                                              .Where(x => String.Equals(x.propName, "id", StringComparison.CurrentCultureIgnoreCase) &&
                                                          ((x.propVal != null) && (x.propVal.Length < 10))) // Ignore special ids > 10 characters long
                                              .Select(x => x.propVal);

          // Grab the object properties
          return(GetAddressBooksEntryForIds(ids.ToArray(), selectProps));
        }, "entry", selectProps));
      }
      catch (Exception e) {
        Trace.TraceError(e.Message);
        Trace.TraceError(e.StackTrace);

        throw (new RicohOperationFailedException(this, "SearchDirectory", e));
      }
    }

    /// <summary>
    /// Get the complete address book at the remote endpoint.
    /// </summary>
    /// <param name="selectProps">The properties to retrieve.</param>
    public IEnumerable<AddressBookEntry> GetAddressBook(string[] selectProps = null)
    {
      return (Connect(() => {
        IList<AddressBookEntry> entries = SearchDirectory(selectProps: selectProps).ToList();
        Trace.TraceInformation("Entries retrieved: {0}", String.Join(",", entries.Select(x => x.ToString())));

        return (entries);
      }));
    }

    /// <summary>
    /// Disconnects / releases the session key for the external service.
    /// </summary>
    /// <returns>True if the session release was acknowledged by the external service, false if otherwise.</returns>
    public bool Disconnect(bool wasAutoConnected = false)
    {
      if (! IsConnected) return (false);

      try {
        if (IsOK(Client.terminateSession(SessionId))) {
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
    /// Checks to see if the device supports tags.
    /// </summary>
    private bool IsTagSupported()
    {
      if (_capabilities == null) return(false);
      uint maxTagPerEntry;
      if (UInt32.TryParse(_capabilities.GetValue("maxTagPerEntry"), out maxTagPerEntry)) {
        return(maxTagPerEntry > 1);
      }
      return (false);
    }

    /// <summary>
    /// Get the tags that are recognized / supported by this device.
    /// </summary>
    /// <param name="parentObjectId">The parent object Id to retrieve the tags from. Defaults to 2</param>
    public IDictionary<uint, string> GetTags(uint parentObjectId = 2)
    {
      if (! IsTagSupported()) return (new Dictionary<uint, string>());

      var tags = Search((properties) => {
        uint id = 0;

        IList<Tuple<uint, string>> result = new List<Tuple<uint, string>>();
        foreach (var item in properties) {
          if (UInt32.TryParse(item.GetValue("id"), out id)) {
            result.Add(new Tuple<uint, string>(id, item.GetValue("label")));
          }
        }        
        return (result);
      }, "tag", new[] { "id", "label" }, parentObjectId : parentObjectId);
      return (tags.ToDictionary(t => t.Item1, t => t.Item2));
    }

    /// <summary>
    /// Get the tag id of the tag that matches the name.
    /// </summary>
    /// <param name="name">The name to lookup the corresponding tag id for.</param>
    private uint? GetTagId(string name)
    {
      if (String.IsNullOrEmpty(name)) return(null);

      var tags = GetTags();
      if (tags.Count <= 0) return(null);

      return (tags.SingleOrDefault(t => t.Value.ToCharArray().Contains(Char.ToUpper(name[0]))).Key);
    }

    /// <summary>
    /// Set the tag id of the tag that matches the entry book address properties. If
    /// no email address is assigned, then no address book entry is used. Otherwise
    /// the first character of the displayName is used.
    /// </summary>
    /// <param name="entry">The address book entry to set the tag id for.</param>
    private void SetTagId(AddressBookEntry entry)
    {
      if (entry == null) return;
      if (String.IsNullOrEmpty(entry.EmailAddress)) return;

      uint? tagId = GetTagId(entry.DisplayName);
      if ((tagId != null) && (tagId > 0)) {
        entry.Set("tagId", String.Format("1,{0}", tagId));
      }
    }

    /// <summary>
    /// Go through the properties and ensure that they are properly prepare for external
    /// consumption.
    /// </summary>
    /// <param name="properties">The properties to prepare.</param>
    /// <returns>The resulting list of prepared properties.</returns>
    private property[] PrepareProperties(IEnumerable<property> properties)
    {
      // An action to truncate any fields as necessary
      Action<property, property> truncateString = (p, c) => {
        if ((c != null) & (p != null)) {
          var maxLength = Convert.ToInt32(c.propVal);
          if ((p.propVal ?? String.Empty).Length > maxLength) {
            p.propVal = (p.propVal ?? String.Empty).Substring(0, maxLength);
          }
        }
      };
      
      // Maps the names to the actions.
      IDictionary<string, Tuple<string, Action<property, property>>> conversions = new Dictionary<string, Tuple<string, Action<property, property>>> {
        { "name", new Tuple<string, Action<property, property>>("maxEntryNameSize", truncateString) },
        { "longName", new Tuple<string, Action<property, property>>("maxEntryLongNameSize", truncateString) },
        { "mail:address", new Tuple<string, Action<property, property>>("maxMailAddressLength", truncateString) }
      };

      properties.AsParallel().ForAll(p => {
        if (conversions.ContainsKey(p.propName)) {
          var conversion = conversions[p.propName];
          if (conversion != null) {
            conversion.Item2.Invoke(p, _capabilities.Get(conversion.Item1));
          }
        }
      });
      return (properties.ToArray());
    }

    /// <summary>
    /// Adds a user to the directory.
    /// </summary>
    /// <param name="name">The full name of th user.</param>
    /// <param name="userCode">The authentication code used for the user.</param>
    /// <param name="displayName">The display name on the copier control panel for the user.</param>
    /// <param name="emailAddress">The email address of the user to add.</param>
    /// <returns>The index of the newly created user.</returns>
    public uint AddUser(string name, string userCode, string displayName, string emailAddress = null)
    {
      return (AddUser(name, userCode, displayName, emailAddress, SetTagId));
    }
    
    /// <summary>
    /// Adds a user to the directory.
    /// </summary>
    /// <param name="name">The full name of th user.</param>
    /// <param name="userCode">The authentication code used for the user.</param>
    /// <param name="displayName">The display name on the copier control panel for the user.</param>
    /// <param name="emailAddress">The email address of the user to add.</param>
    /// <param name="options">Allows for additional processing and manipulation to occur before adding the user. Requires internal knowledge of the external service proxy.</param>
    /// <returns>The index of the newly created user.</returns>
    public uint AddUser(string name, string userCode, string displayName, string emailAddress = null, Action<AddressBookEntry> options = null)
    {
      if (String.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty or null.", name);

      AddressBookEntry entry = new AddressBookEntry(RicohEntryType.User) { Name = name };
      if (!String.IsNullOrEmpty(userCode)) entry.Usercode = userCode;
      if (!String.IsNullOrEmpty(displayName)) entry.DisplayName = displayName;
      if (!String.IsNullOrEmpty(emailAddress)) entry.EmailAddress = emailAddress;

      return (Connect(() => {
        property[] defaultProperties = new[] {
          new property() { propName = "isDestination", propVal = "true" }, 
          new property() { propName = "isSender", propVal = "true" }, 
        };
        try {
          if (options != null) options.Invoke(entry);

          var properties = PrepareProperties(defaultProperties.Concat(entry.ToProperties()));
          var result = Client.putObjects(SessionId, "entry", null, new property[][] { properties }, null);
          return (result.Length > 0 ? Convert.ToUInt32(result[0]) : 0);
        }
        catch (FaultException ex) {
          // TODO: Need to inspect the returned XML message from the server to check for UDIRECTORY_DIRECTORY_INCONSISTENT
          Trace.TraceError("Failed to add the address book entry for: {0} [{1}]. Possibly duplicate usercode?", entry.DisplayName, entry.Usercode);
          return ((uint) 0);
        }
      }, RicohSessionType.ExclusiveSession));
    }

    /// <summary>
    /// Deletes the specified entry.
    /// </summary>
    /// <param name="entry">The id to remove.</param>
    /// <returns>True if removed, false if failed.</returns>
    public bool Delete(uint entry)
    {
      return (Delete(new[] { entry }) == 1);
    }

    /// <summary>
    /// Deletes the specified entries.
    /// </summary>
    /// <param name="entries">The collection of IDs to remove.</param>
    /// <returns>The number of entries successfully removed.</returns>
    public uint Delete(IEnumerable<uint> entries)
    {
      if ((entries == null) || (!entries.Any())) return (0);

      return (Connect(() => {
        try {
          return(Convert.ToUInt32(Client.deleteObjects(SessionId, entries.Select(e => String.Format("entry:{0}", e)).ToArray(), null)));
        } catch (FaultException ex) {
          // TODO: Need to inspect the returned XML message from the server to check for UDIRECTORY_DIRECTORY_INCONSISTENT
          Trace.TraceError("Failed to remove one or more of the address book entries: {0}.", String.Join(",", entries.Select(e => e.ToString())));
          return ((uint) 0);
        }
      }, RicohSessionType.ExclusiveSession));
    }

    /// <summary>
    /// Deletes the specified entry.
    /// </summary>
    /// <param name="entry">The id to remove.</param>
    /// <returns>True if removed, false if failed.</returns>
    public bool Delete(AddressBookEntry entry)
    {
      return (Delete(new[] { entry }) == 1);
    }

    /// <summary>
    /// Deletes the specified entries.
    /// </summary>
    /// <param name="entries">The collection of address book entries to remove.</param>
    /// <returns>The number of entries successfully removed.</returns>
    public uint Delete(IEnumerable<AddressBookEntry> entries)
    {
      if ((entries == null) || (!entries.Any())) return (0);
      return(Delete(entries.Select(e => Convert.ToUInt32(e.Id))));
    }

    /// <summary>
    /// Occurs when an address book entry has be retrieved.
    /// </summary>
    /// <param name="entry">The address book entry that was retrieved.</param>
    protected virtual void OnAddressBookEntryRetrieved(AddressBookEntry entry)
    {
      if (AddressBookEntryRetrieved != null) {
        AddressBookEntryRetrieved(this, entry);
      }
    }

    /// <summary>
    /// Occurs when an address book entry has be added.
    /// </summary>
    /// <param name="id">The ID of the address book entry that was added.</param>
    protected virtual void OnAddressBookEntryAdded(uint id)
    {
      if (AddressBookEntryAdded != null) {
        // Go grab the newly created entry.
        var entry = GetAddressBookEntry(id);
        if (entry != null) {
          AddressBookEntryAdded(this, entry);
        }
      }
    }

    /// <summary>
    /// Occurs when an address book entry has been removed.
    /// </summary>
    /// <param name="id">The id of the address book entry that was removed.</param>
    protected virtual void OnAddressBookEntryRemoved(uint id)
    {
      if (AddressBookEntryRemoved != null) {
        AddressBookEntryRemoved(this, id);
      }
    }
    
    /// <summary>
    /// Occurs when an address book entry is invalid.
    /// </summary>
    /// <param name="values">The key value pairs of the invalid address book entry.</param>
    protected virtual void OnInvalidAddressBookEntry(IDictionary values)
    {
      if (InvalidAddressBookEntry != null) {
        InvalidAddressBookEntry(this, values);
      }
    }
  }
}