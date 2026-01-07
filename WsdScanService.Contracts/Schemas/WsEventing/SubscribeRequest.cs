using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Xml.Schema;
using System.Xml.Serialization;
using Windows.Wdp.Scan;
using WsdScanService.Common;
using WsdScanService.Contracts.Schemas.WsAddressing;

namespace WsdScanService.Contracts.Schemas.WsEventing;

[MessageContract(WrapperName = "Subscribe", WrapperNamespace = Xd.Eventing.Namespace, IsWrapped = true)]
[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class SubscribeRequest
{
    [MessageBodyMember(Order = 0)]
    public required EndpointReference<EventingReferenceParameters, ReferenceProperties> EndTo { get; set; }

    [MessageBodyMember(Order = 1)] public required Delivery Delivery { get; set; }

    [MessageBodyMember(Order = 2)] public string? Expires { get; set; }

    [MessageBodyMember(Order = 3)] public required Filter Filter { get; set; }

    [MessageBodyMember(Order = 4, Namespace = Xd.ScanService.Namespace)]
    public required ScanDestinationsType ScanDestinations { get; set; }
}

[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class LanguageSpecificStringType
{
    [XmlAttribute(AttributeName = "lang",
        Form = XmlSchemaForm.Qualified,
        Namespace = "http://www.w3.org/XML/1998/namespace"
    )]
    public required string? Lang { get; set; }

    [XmlText] public required string Value { get; set; }
}

[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class Filter
{
    [XmlText] public required Collection<string> Text { get; set; }

    [XmlAttribute(DataType = "anyURI")] public required string Dialect { get; set; }
}

[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class Delivery
{
    [XmlAttribute(DataType = "anyURI")] public required string Mode { get; set; }

    [XmlElement]
    public required EndpointReference<EventingReferenceParameters, ReferenceProperties> NotifyTo { get; set; }
}

[XmlRoot(Namespace = Xd.Eventing.Namespace)]
public class EventingReferenceParameters : ReferenceParameters
{
    [XmlElement] public required string Identifier { get; set; }
}