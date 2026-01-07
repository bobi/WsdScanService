using System.ServiceModel;
using System.Xml.Serialization;
using Ws.Mex;
using WsdScanService.Common;

namespace WsdScanService.Contracts.Schemas.WsTransfer;

[MessageContract(IsWrapped = false)]
[XmlRoot(Namespace = Xd.MetadataExchange.Namespace)]
public class GetResponse
{
    [MessageBodyMember(Order = 0)]
    [XmlElement(Namespace = Xd.MetadataExchange.Namespace)]
    public required Metadata Metadata { get; set; }
}