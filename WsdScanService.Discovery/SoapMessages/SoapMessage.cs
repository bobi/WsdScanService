using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Envelope", Namespace = ProtocolConstants.Namespaces.Soap)]
public class SoapMessage<TBody> where TBody : class
{
    [XmlElement(ElementName = "Header", Namespace = ProtocolConstants.Namespaces.Soap)]
    public SoapHeader? SoapHeader;

    [XmlElement(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
    public TBody? SoapBody;

    public static SoapMessage<TBody> Create(SoapHeader? header = null, TBody? body = null)
    {
        return new SoapMessage<TBody>
        {
            SoapHeader = header,
            SoapBody = body
        };
    }
}

[XmlRoot(ElementName = "Header", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
public class SoapHeader
{
    [XmlElement(ElementName = "Action", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public required string Action;

    [XmlElement(ElementName = "MessageID", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public required string MessageId;

    [XmlElement(ElementName = "RelatesTo", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public string? RelatesTo;

    [XmlElement(ElementName = "To", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public string? To;

    [XmlElement(ElementName = "AppSequence", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public required AppSequence AppSequence;

    // [field: XmlElement(ElementName = "From")]
    // public EndpointReference? From;

    public static SoapHeader Create(string action, string messageId, int instanceId)
    {
        return new SoapHeader
        {
            Action = action,
            MessageId = messageId,
            AppSequence = AppSequence.Create(instanceId)
        };
    }
}

[XmlType(Namespace = ProtocolConstants.Namespaces.Soap)]
public abstract class SoapBody;