using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Ricoh
{
  /// <summary>
  /// A message inspector that modifies the outgoing RPC message requests to comply with the
  /// Ricoh service.
  /// </summary>
  public class RicohEmbeddedMessageInspector : IClientMessageInspector
  {
    /// <summary>
    /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
    /// </summary>
    /// <param name="reply">The message to be transformed into types and handed back to the client application.</param><param name="correlationState">Correlation state data.</param>
    public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
    {
    }

    /// <summary>
    /// Enables inspection or modification of a message before a request message is sent to a service.
    /// </summary>
    /// <returns>
    /// The object that is returned as the <paramref name="correlationState "/>argument of the <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)"/> method. This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid"/> to ensure that no two <paramref name="correlationState"/> objects are the same.
    /// </returns>
    /// <param name="request">The message to be sent to the service.</param><param name="channel">The WCF client object channel.</param>
    public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
    {
      XmlDocument doc = new XmlDocument();

      MemoryStream ms = new MemoryStream();
      using (XmlWriter writer = XmlWriter.Create(ms)) {
        request.WriteMessage(writer);
      }

      ms.Position = 0;
      doc.Load(ms);
      Trace.TraceInformation("RicohEmbeddedMessageInspector.BeforeSendRequest INCOMING={0}", doc.OuterXml);
      FixEmbeddedMessageBody(doc);
      Trace.TraceInformation("RicohEmbeddedMessageInspector.BeforeSendRequest OUTGOING={0}", doc.OuterXml);
      ms.SetLength(0);

      using (XmlWriter writer = XmlWriter.Create(ms)) {
        doc.WriteTo(writer);
      }

      ms.Position = 0;
      XmlReader reader = XmlReader.Create(ms);
      request = Message.CreateMessage(reader, int.MaxValue, request.Version);

      return null;
    }

    /// <summary>
    /// The .NET XML serializer generate HREFs to complex objects. The remote service doesn't understand
    /// how to process this so we need to rewrite the XML message and embed the elements inline.
    /// </summary>
    /// <param name="document">The Xml document to inspect.</param>
    /// <returns>The modified Xml document.</returns>
    private XmlDocument FixEmbeddedMessageBody(XmlDocument document)
    {
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
      namespaceManager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
      XmlNode header = document.SelectSingleNode("s:Envelope/s:Header", namespaceManager);
      if (header != null) {
        string action = header.InnerText.Substring(header.InnerText.LastIndexOf('#') + 1);
        XmlNode body = document.SelectSingleNode("s:Envelope/s:Body", namespaceManager);
        // Does our body have more than one element? If so, then we need to merge the extra
        // elements into the main node body. The external service doesn't recognize the href
        // attributes.
        if (body != null) {
          int childIndex = 0;
          while (body.ChildNodes.Count > 1) {
            XmlNode child = body.ChildNodes[childIndex];

            // If the node name doesn't equal the name of the service action, then it's one of
            // our extra node which need to be merged.
            if (!String.Equals(action, child.LocalName, StringComparison.CurrentCultureIgnoreCase)) {
              XmlNode orphan = body.RemoveChild(child);
              if (orphan.Attributes != null) {
                string id = Convert.ToString(orphan.Attributes["id"].Value);
                if (!String.IsNullOrEmpty(id)) {
                  orphan.Attributes.Remove(orphan.Attributes["id"]);

                  // Find the #href node in the action.
                  XmlNode node = document.SelectSingleNode(String.Format("s:Envelope/s:Body//*[@href='#{0}']", id), namespaceManager);
                  if (node != null) {
                    if (node.Attributes != null) {
                      node.Attributes.Remove(node.Attributes["href"]);
                      for (int index = 0, length = orphan.Attributes.Count; index < length; index++) {
                        node.Attributes.Append(orphan.Attributes[0]);
                      }
                    }
                    node.InnerXml = orphan.InnerXml;
                  }
                }
              }
            }
            else childIndex++;
          }
        }
      }
      return (document);
    }
  }
}