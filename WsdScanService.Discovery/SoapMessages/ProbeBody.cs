using System.Xml;
using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ProbeBody : SoapBody
{
    [XmlElement("Probe", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public required Probe Probe;

    public static ProbeBody Create(XmlQualifiedName[]? types = null, string[]? scopes = null) => new()
    {
        Probe = new Probe
        {
            Types = types != null ? string.Join(" ", types.Select(DiscoveryXmlSerializerNamespaces.GetPrefixed)) : null,
            Scopes = scopes != null ? string.Join(" ", scopes) : null,
        }
    };
}

[XmlRoot(ElementName = "Probe", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class Probe
{
    [XmlElement("Types")] public string? Types;
    [XmlElement("Scopes")] public string? Scopes;
}