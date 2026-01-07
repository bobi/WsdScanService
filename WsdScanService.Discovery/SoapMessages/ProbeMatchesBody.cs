using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ProbeMatchesBody : SoapBody
{
#pragma warning disable 0649
    [XmlElement(ElementName = "ProbeMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ProbeMatches? ProbeMatches;
#pragma warning restore 0649
}

[XmlRoot(ElementName = "ProbeMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ProbeMatches
{
#pragma warning disable 0649
    [XmlElement(ElementName = "ProbeMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ProbeMatch[]? Matches;
#pragma warning restore 0649
}

[XmlRoot(ElementName = "ProbeMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ProbeMatch
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
    [XmlElement(ElementName = "XAddrs", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? XAddrs;
#pragma warning restore 0649

#pragma warning disable 0649
    [XmlElement(ElementName = "MetadataVersion", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public uint? MetadataVersion;
#pragma warning restore 0649
}