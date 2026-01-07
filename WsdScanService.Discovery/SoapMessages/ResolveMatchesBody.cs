using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ResolveMatchesBody : SoapBody
{
#pragma warning disable 0649
    [XmlElement(ElementName = "ResolveMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ResolveMatches? ResolveMatches;
#pragma warning restore 0649
}

[XmlRoot(ElementName = "ResolveMatches", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ResolveMatches
{
#pragma warning disable 0649
    [XmlElement(ElementName = "ResolveMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public ResolveMatch? ResolveMatch;
#pragma warning restore 0649
}

[XmlRoot(ElementName = "ResolveMatch", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class ResolveMatch
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