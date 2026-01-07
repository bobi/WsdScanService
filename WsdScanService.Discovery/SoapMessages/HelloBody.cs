using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class HelloBody : SoapBody
{
    [XmlElement(ElementName = "Hello", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public Hello? Hello;
}

[XmlRoot(ElementName = "Hello", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class Hello
{
    [XmlElement(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public EndpointReference? EndpointReference;

    [XmlElement(ElementName = "Types", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? Types;

    [XmlElement(ElementName = "Scopes", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public object? Scopes;

    [XmlElement(ElementName = "XAddrs", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? XAddrs;

    [XmlElement(ElementName = "MetadataVersion", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public int? MetadataVersion;
}