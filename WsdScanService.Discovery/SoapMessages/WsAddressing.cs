using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)]
public  class EndpointReference
{
#pragma warning disable 0649
    [XmlElement("Address")] public required string Address;
#pragma warning restore 0649
    
#pragma warning disable 0649
    [XmlElement("ReferenceProperties")] public Collection<XmlElement>? ReferenceProperties;
#pragma warning restore 0649
    
#pragma warning disable 0649
    [XmlElement("ReferenceParameters")] public Collection<XmlElement>? ReferenceParameters;
#pragma warning restore 0649
    
    // [XmlElement("PortType")] public XmlQName? PortType;

    // [XmlElement("ServiceName")] public ServiceNameType? ServiceName;
}