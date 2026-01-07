using System.ServiceModel;
using System.Xml.Serialization;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.WsdSchemas.WsEventing;

[MessageContract(WrapperName = "UnsubscribeResponse", WrapperNamespace = Xd.Eventing.Namespace, IsWrapped = true)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class UnsubscribeResponse;