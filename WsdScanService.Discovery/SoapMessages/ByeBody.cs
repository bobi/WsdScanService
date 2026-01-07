using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.Soap)]
public class ByeBody { 

    [XmlElement(ElementName="Bye", Namespace = ProtocolConstants.Namespaces.WsDiscovery)] 
    public Bye? Bye; 
}

[XmlRoot(ElementName = "Body", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class Bye { 

    [XmlElement(ElementName = "EndpointReference", Namespace = ProtocolConstants.Namespaces.WsAddressing)] 
    public EndpointReference? EndpointReference; 
}