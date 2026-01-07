using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ByeBody
{
#pragma warning disable 0649
    [XmlElement(ElementName = "Bye", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
    public Bye? Bye;
#pragma warning restore 0649
}

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class Bye
{
#pragma warning disable 0649
    [XmlElement(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
    public EndpointReference? EndpointReference;
#pragma warning restore 0649
}