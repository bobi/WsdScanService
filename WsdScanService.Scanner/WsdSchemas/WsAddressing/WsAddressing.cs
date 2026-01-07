using System.Xml;
using System.Xml.Serialization;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.WsdSchemas.WsAddressing;

[XmlRoot(Namespace = Xd.Addressing200408.Namespace)]
public class EndpointReference<TReferenceParameters, TReferenceProperties> 
    where TReferenceParameters : ReferenceParameters where TReferenceProperties : ReferenceProperties
{
    [XmlElement(Order = 0)]
    public required string Address { get; set; }

    [XmlElement(Order = 1)]
    public TReferenceProperties? ReferenceProperties { get; set; }

    [XmlElement(Order = 2)]
    public TReferenceParameters? ReferenceParameters { get; set; }

    [XmlElement(Order = 3)]
    public ServiceName? ServiceName { get; set; }
}

[XmlRoot(Namespace = Xd.Addressing200408.Namespace)]
public class ServiceName
{
    [XmlAttribute(DataType = "NCName")]
    public required string PortName { get; set; }

    [XmlText] public required XmlQualifiedName Value { get; set; }
}

public abstract class ReferenceParameters;

public abstract class ReferenceProperties;