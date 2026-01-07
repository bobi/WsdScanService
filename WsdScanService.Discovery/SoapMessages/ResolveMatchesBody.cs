using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ResolveMatchesBody : SoapBody
{
    [XmlElement(ElementName = "ResolveMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ResolveMatches? ResolveMatches;
}

[XmlRoot(ElementName = "ResolveMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ResolveMatches
{
    [XmlElement(ElementName = "ResolveMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ResolveMatch? ResolveMatch;
}

[XmlRoot(ElementName = "ResolveMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ResolveMatch
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
