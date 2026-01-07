using System.Xml.Serialization;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.SoapMessages;

[XmlRoot(ElementName = "AppSequence", Namespace = ProtocolConstants.Namespaces.WsDiscovery)]
public class AppSequence
{
    [XmlAttribute(AttributeName = "InstanceId", Namespace = "")]
    public uint InstanceId;

    [XmlAttribute(AttributeName = "MessageNumber", Namespace = "")]
    public uint MessageNumber;

    public static AppSequence Create(uint instanceId)
    {
        return new AppSequence
        {
            InstanceId = instanceId,
            MessageNumber = Convert.ToUInt32(Environment.TickCount)
        };
    }
}