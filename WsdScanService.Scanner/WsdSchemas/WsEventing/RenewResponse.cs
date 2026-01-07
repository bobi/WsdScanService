using System.ServiceModel;
using System.Xml.Serialization;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.WsdSchemas.WsEventing;

[MessageContract(WrapperName = "RenewResponse", WrapperNamespace = Xd.Eventing.Namespace, IsWrapped = true)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class RenewResponse
{
    [MessageHeader(MustUnderstand = true, Namespace = Xd.Eventing.Namespace)]
    public required string Identifier { get; set; }

    [MessageBodyMember(Order = 0)]
    public string? Expires { get; set; }
}
