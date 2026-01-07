using System.Collections.ObjectModel;
using System.Xml;

namespace WsdScanService.Common.Utils;

public static class XmlUtils
{
    public static XmlQualifiedName? ParseQName(string? qName, XmlNamespaceManager? namespaceManager)
    {
        var strings = qName?.Split(':', StringSplitOptions.RemoveEmptyEntries);

        return strings?.Length != 2
            ? null
            : new XmlQualifiedName(strings[1], namespaceManager?.LookupNamespace(strings[0]) ?? string.Empty);
    }

    public static XmlQualifiedName[]? ParseTypes(string? types, XmlNamespaceManager? namespaceManager)
    {
        return types?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => ParseQName(s, namespaceManager))
            .OfType<XmlQualifiedName>()
            .ToArray();
    }

    public static XmlQualifiedName[]? ParseTypes(Collection<string>? types, XmlNamespaceManager? namespaceManager)
    {
        return types?
            .Select(s => ParseQName(s, namespaceManager))
            .OfType<XmlQualifiedName>()
            .ToArray();
    }
}