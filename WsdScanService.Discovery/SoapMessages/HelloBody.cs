using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class HelloBody : SoapBody
{
#pragma warning disable 0649
    [XmlElement(ElementName = "Hello", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public Hello? Hello;
#pragma warning restore 0649
}

[XmlRoot(ElementName = "Hello", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class Hello
{
#pragma warning disable 0649
    [XmlElement(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public EndpointReference? EndpointReference;
#pragma warning restore 0649

#pragma warning disable 0649
    [XmlElement(ElementName = "Types", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? Types;
#pragma warning restore 0649

#pragma warning disable 0649
    [XmlElement(ElementName = "Scopes", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public object? Scopes;
#pragma warning restore 0649

#pragma warning disable 0649
    [XmlElement(ElementName = "XAddrs", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? XAddrs;
#pragma warning restore 0649

#pragma warning disable 0649
    [XmlElement(ElementName = "MetadataVersion", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public uint? MetadataVersion;
#pragma warning restore 0649
}