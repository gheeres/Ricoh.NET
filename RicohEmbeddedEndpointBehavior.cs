using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Ricoh
{
  /// <summary>
  /// Specifies an endpoint behavior that automatically applies the RicohEmbeddedMessageInspector
  /// </summary>
  public class RicohEmbeddedEndpointBehavior : BehaviorExtensionElement, IEndpointBehavior
  {
    /// <summary>Gets the type of behavior.</summary>
    public override Type BehaviorType
    {
      get { return typeof(RicohEmbeddedEndpointBehavior); }
    }

    /// <summary>
    /// Creates a behavior extension based on the current configuration settings.
    /// </summary>
    /// <returns>
    /// The behavior extension.
    /// </returns>
    protected override object CreateBehavior()
    {
      // Create the endpoint behavior that will insert the message
      // inspector into the client runtime
      return (new RicohEmbeddedEndpointBehavior());
    }

    /// <summary>
    /// Implement to pass data at runtime to bindings to support custom behavior.
    /// </summary>
    /// <param name="endpoint">The endpoint to modify.</param><param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    /// <summary>
    /// Implements a modification or extension of the client across an endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint that is to be customized.</param><param name="clientRuntime">The client runtime to be customized.</param>
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
      clientRuntime.MessageInspectors.Add(new RicohEmbeddedMessageInspector());
    }

    /// <summary>
    /// Implements a modification or extension of the service across an endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint that exposes the contract.</param><param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    /// <summary>
    /// Implement to confirm that the endpoint meets some intended criteria.
    /// </summary>
    /// <param name="endpoint">The endpoint to validate.</param>
    public void Validate(ServiceEndpoint endpoint)
    {
    }
  }
}