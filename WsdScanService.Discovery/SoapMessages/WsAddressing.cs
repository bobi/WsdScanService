using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
public class EndpointReference
{
    [XmlElement("Address")] public required string Address;

    [XmlElement("ReferenceProperties")] public Collection<XmlElement>? ReferenceProperties;

    [XmlElement("ReferenceParameters")] public Collection<XmlElement>? ReferenceParameters;

    // [XmlElement("PortType")] public XmlQName? PortType;

    // [XmlElement("ServiceName")] public ServiceNameType? ServiceName;
}