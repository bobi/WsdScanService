using System.ServiceModel;
using System.Xml.Serialization;
using WsdScanService.Common;

namespace WsdScanService.Contracts.Schemas.WsEventing;

[MessageContract(WrapperName = "UnsubscribeResponse", WrapperNamespace = Xd.Eventing.Namespace, IsWrapped = true)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class UnsubscribeResponse;