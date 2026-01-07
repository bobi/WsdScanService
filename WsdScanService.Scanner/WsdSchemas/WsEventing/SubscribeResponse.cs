using System.ServiceModel;
using System.Xml.Serialization;
using Windows.Wdp.Scan;
using WsdScanService.Scanner.Contracts;
using WsdScanService.Scanner.WsdSchemas.WsAddressing;

namespace WsdScanService.Scanner.WsdSchemas.WsEventing;

[MessageContract(WrapperName = "SubscribeResponse", WrapperNamespace = Xd.Eventing.Namespace, IsWrapped = true)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class SubscribeResponse
{
    [MessageBodyMember(Order = 0)]
    public required EndpointReference<EventingReferenceParameters, ReferenceProperties> SubscriptionManager { get; set; }

    [MessageBodyMember(Order = 0)]
    public string? Expires { get; set; }
    
    [MessageBodyMember(Order = 4, Namespace = Xd.ScanService.Namespace)]
    public required DestinationResponsesType? DestinationResponses { get; set; }
}
