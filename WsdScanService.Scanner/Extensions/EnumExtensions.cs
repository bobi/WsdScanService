using System.Xml;
using System.Xml.Serialization;

namespace WsdScanService.Scanner.Extensions;

internal static class EnumExtensions
{
    public static XmlQualifiedName ToXmlQualifiedName(this Enum value)
    {
        var enumType = value.GetType();
        var fieldName = Enum.GetName(enumType, value);
        
        var xmlTypeAttributes = enumType.GetCustomAttributes(typeof(XmlTypeAttribute), false);
        var ea = xmlTypeAttributes.FirstOrDefault();

        if (ea is XmlTypeAttribute xmlTypeAttribute)
        {
            return new XmlQualifiedName(fieldName, xmlTypeAttribute.Namespace);
        }

        return new XmlQualifiedName(fieldName, string.Empty);
    }

    public static string ToSoapAction(this Enum value)
    {
        return string.Empty;
    }
}