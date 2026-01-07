using System.ServiceModel;
using System.Xml.Serialization;
using WsdScanService.Common;

namespace WsdScanService.Contracts.Schemas.WsEventing;

[MessageContract(IsWrapped = false)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class UnsubscribeRequest
{
    [MessageHeader(MustUnderstand = true, Namespace = Xd.Eventing.Namespace)]
    public required string Identifier { get; set; }
}