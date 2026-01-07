using System.ServiceModel;
using System.Xml.Serialization;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.WsdSchemas.WsEventing;

[MessageContract(WrapperName = "Renew", WrapperNamespace = Xd.Eventing.Namespace, IsWrapped = true)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class RenewRequest
{
    [MessageHeader(MustUnderstand = true, Namespace = Xd.Eventing.Namespace)]
    public required string Identifier { get; set; }

    [MessageBodyMember(Order = 0)] 
    public string? Expires { get; set; }
}
