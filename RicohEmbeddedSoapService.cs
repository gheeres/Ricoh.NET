using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Ricoh
{
  public abstract class RicohEmbeddedSoapService: IDisposable
  {
    protected const ushort DEFAULT_TIMELIMIT = 30;

    private string _hostname = "127.0.0.1";
    private string _username = "admin";
    private ushort _timeLimit = DEFAULT_TIMELIMIT;

    /// <summary>The session id that identifies us to the copier.</summary>
    public virtual string SessionId { get; protected set; }

    /// <summary>The hostname or ip address of the copier to connect to.</summary>
    public virtual string Hostname
    { 
      get { return(_hostname); }
      protected set { _hostname = value; }
    }
    /// <summary>The username for authentication. Default: "admin".</summary>
    public virtual string Username
    {
      get { return(_username); } 
      protected set { _username = value; }
    }
    /// <summary>The password for authentiation.</summary>
    public virtual string Password { get; set; }
    /// <summary>The time limit for our sessionId in ??minutes / seconds??. Default: 30</summary>
    public virtual ushort TimeLimit
    {
      get { return(_timeLimit); }
      set { _timeLimit = value; }
    }

    /// <summary>Indicates if connected to the directory.</summary>
    public virtual bool IsConnected
    {
      get { return (! String.IsNullOrEmpty(SessionId)); }
    }
    
    /// <param name="hostname">The hostname or ip address of the copier to connect to.</param>
    /// <param name="username">The username for authentication. Default: "admin".</param>
    /// <param name="password">The password for authentiation.</param>
    protected RicohEmbeddedSoapService(string hostname, string username = "admin", string password = null)
    {
      Hostname = hostname;
      Username = username;
      Password = password;
    }

    /// <summary>
    /// GetValue's the authentication string / scheme used by the ricoh service.
    /// </summary>
    /// <param name="scheme">The name of the service.</param>
    /// <param name="username">The name of the user to authentication. If not specified, then the preconfigured username is used.</param>
    /// <param name="password">The password to authenticate with. If not specified, then the preconfigured password is used.</param>
    /// <returns>The unique authentication string required by the ricoh service.</returns>
    protected virtual string GetAuthenticationScheme(string scheme = "BASIC", string username = null, string password = null)
    {
      return (String.Format("SCHEME={0};UID:UserName={1};PWD:Password={2};PES:Encoding=", 
                            (scheme ?? "BASIC").ToBase64(), 
                            (username ?? Username).ToBase64(), 
                            (password ?? Password).ToBase64()));
    }

    /// <summary>
    /// GetValue's the endpoint address for the connection.
    /// </summary>
    /// <returns>The configured endpoint address.</returns>
    protected abstract EndpointAddress GetEndpointAddress();

    /// <summary>
    /// GetValue's the endpoint behavior for service proxy.
    /// </summary>
    /// <returns>The configured endpoint behavoir.</returns>
    protected virtual IEnumerable<IEndpointBehavior> GetEndpointBehaviors()
    {
      return (new [] { new RicohEmbeddedEndpointBehavior() });
    }

    /// <summary>
    /// GetValue's the endpoint binding.
    /// </summary>
    /// <returns>The configured binding.</returns>
    protected virtual Binding GetBinding()
    {
      return (new BasicHttpBinding() {
        MaxReceivedMessageSize = Int32.MaxValue
      });
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public abstract void Dispose();

    /// <summary>
    /// Checks to see if the response is recognized as a valid OK response.
    /// </summary>
    /// <param name="response">The response to check.</param>
    /// <param name="acceptableValues">The optional array of acceptable values that pass the truth test.</param>
    /// <returns>True if the response is OK, false if otherwise.</returns>
    protected virtual bool IsOK(string response, IEnumerable<string> acceptableValues = null)
    {
      if (acceptableValues == null) acceptableValues = new string[] { RicohResponseStatus.OK };
      return(String.IsNullOrEmpty(response) || acceptableValues.Contains(response, StringComparer.CurrentCultureIgnoreCase));
    }
  }
}