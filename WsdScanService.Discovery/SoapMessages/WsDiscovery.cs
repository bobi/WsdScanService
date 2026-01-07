using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName="AppSequence", Namespace=ProtocolConstants.Namespaces.WsDiscovery)]
public class AppSequence { 

    [XmlAttribute(AttributeName="InstanceId", Namespace="")] 
    public int InstanceId; 

    [XmlAttribute(AttributeName="MessageNumber", Namespace="")] 
    public int MessageNumber;
    
    public static AppSequence Create(int instanceId)
    {
        return new AppSequence
        {
            InstanceId = instanceId,
            MessageNumber = Convert.ToInt32(Environment.TickCount)
        };
    }
}