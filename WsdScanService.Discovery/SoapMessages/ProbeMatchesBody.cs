using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ProbeMatchesBody: SoapBody
{
    [XmlElement(ElementName = "ProbeMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ProbeMatches? ProbeMatches;
}

[XmlRoot(ElementName = "ProbeMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ProbeMatches
{
    [XmlElement(ElementName = "ProbeMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ProbeMatch[]? Matches;
}

[XmlRoot(ElementName = "ProbeMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ProbeMatch
{
    [XmlElement(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public EndpointReference? EndpointReference;

    [XmlElement(ElementName = "Types", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? Types;

    [XmlElement(ElementName = "XAddrs", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public string? XAddrs;

    [XmlElement(ElementName = "MetadataVersion", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public int? MetadataVersion;
}

