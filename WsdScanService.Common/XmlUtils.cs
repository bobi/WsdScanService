using System.Collections.ObjectModel;
using System.Xml;

namespace WsdScanService.Common;

public static class XmlUtils
{
    public static XmlQualifiedName? ParseQName(string? qName, Func<string, string?>? nsProvider)
    {
        var strings = qName?.Split(':', StringSplitOptions.RemoveEmptyEntries);

        return strings?.Length != 2
            ? null
            : new XmlQualifiedName(strings[1], nsProvider?.Invoke(strings[0]) ?? string.Empty);
    }

    public static XmlQualifiedName[]? ParseTypes(string? types, Func<string, string>? nsProvider = null)
    {
        return types?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => ParseQName(s, nsProvider))
            .OfType<XmlQualifiedName>()
            .ToArray();
    }

    public static XmlQualifiedName[]? ParseTypes(Collection<string>? types, Func<string, string?>? nsProvider = null)
    {
        return types?
            .Select(s => ParseQName(s, nsProvider))
            .OfType<XmlQualifiedName>()
            .ToArray();
    }
}