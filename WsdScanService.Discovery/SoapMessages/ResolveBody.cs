using System.Xml;
using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ResolveBody : SoapBody
{
    [XmlElement(ElementName = "Resolve", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public required Resolve Resolve;

    public static ResolveBody Create(XmlQualifiedName[]? types = null, string[]? scopes = null) => new()
    {
        Resolve = new Resolve
        {
            Types = types,
            Scopes = scopes,
        }
    };
}

[XmlRoot(ElementName = "Resolve", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class Resolve
{
    [XmlIgnore] public XmlQualifiedName[]? Types;
    [XmlElement("Scopes")] public string[]? Scopes;

    [XmlElement("Types")]
    public string? TypesXmlSerialized
    {
        get => string.Join(" ", Types?.Select(qName => qName.ToString()) ?? []);
        set => Types =
            value?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(qName => qName.Split(':', StringSplitOptions.RemoveEmptyEntries))
                .Select(strings => strings.Length != 2
                    ? null
                    : new XmlQualifiedName(strings[1],
                        DiscoveryXmlSerializerNamespaces.GetNamespace(strings[0])))
                .OfType<XmlQualifiedName>()
                .ToArray();
    }
}