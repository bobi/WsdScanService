using System.Xml;

namespace WsdScanService.Discovery.Protocol;

internal static class ProtocolConstants
{
    public static readonly Uri
        MulticastIPv4Address = new(ProtocolStrings.Udp.MulticastIPv4Address); // Standard WS-Discovery multicast address

    public static readonly Uri
        MulticastIPv6Address = new(ProtocolStrings.Udp.MulticastIPv6Address); // Standard WS-Discovery multicast address

    public const string AdhocAddress = ProtocolStrings.VersionApril2005.AdhocAddress;

    internal static class Namespaces
    {
        public const string Soap = "http://www.w3.org/2003/05/soap-envelope"; // SOAP 1.2 envelope

        public const string WsAddressing = ProtocolStrings.WsaNamespaceAugust2004;

        public const string WsDiscovery = ProtocolStrings.VersionApril2005.Namespace;

        public const string DevicesProfile = "http://schemas.xmlsoap.org/ws/2006/02/devprof";

        public const string WsScanner = "http://schemas.microsoft.com/windows/2006/08/wdp/scan";

        public const string NsWsDiscoverySvc = "http://schemas.xmlsoap.org/ws/2005/04/discovery/svc";
    }

    internal static class Actions
    {
        public const string HelloAction = Namespaces.WsDiscovery + "/Hello";
        public const string ByeAction = Namespaces.WsDiscovery + "/Bye";
        public const string ProbeAction = Namespaces.WsDiscovery + "/Probe";
        public const string ProbeMatchesAction = Namespaces.WsDiscovery + "/ProbeMatches";
        public const string ResolveAction = Namespaces.WsDiscovery + "/Resolve";
        public const string ResolveMatchesAction = Namespaces.WsDiscovery + "/ResolveMatches";

        public static ISet<string> All = new HashSet<string>
        {
            HelloAction, ByeAction, ProbeAction, ProbeMatchesAction, ResolveAction
        };

        public const string ScopeMatchByExact = Namespaces.WsDiscovery + "/strcmp0";
        public const string ScopeMatchByLdap = Namespaces.WsDiscovery + "/ldap";
        public const string ScopeMatchByPrefix = Namespaces.WsDiscovery + "/rfc2396";
        public const string ScopeMatchByUuid = Namespaces.WsDiscovery + "/uuid";
        public const string ScopeMatchByNone = ProtocolStrings.Version11.Namespace + "/none";
    }
}

internal static class DiscoveryXmlSerializerNamespaces
{
    public static XmlQualifiedName[] Namespaces { get; } =
    [
        new("soap", ProtocolConstants.Namespaces.Soap),
        new("wsa", ProtocolConstants.Namespaces.WsAddressing),
        new("wsdisco", ProtocolConstants.Namespaces.WsDiscovery),
        new("devprof", ProtocolConstants.Namespaces.DevicesProfile),
        new("wscn", ProtocolConstants.Namespaces.WsScanner)
    ];

    private static readonly Dictionary<string, XmlQualifiedName> NamespacesDictionary =
        Namespaces.ToDictionary(item => item.Namespace, item => item);

    private static readonly Dictionary<string, XmlQualifiedName> PrefixDictionary =
        Namespaces.ToDictionary(item => item.Name, item => item);

    public static string? GetPrefixed(XmlQualifiedName? name)
    {
        if (name == null) return null;

        return GetPrefixed(name.Name, name.Namespace);
    }

    public static string? GetPrefixed(string? localName, string? ns)
    {
        if (ns != null)
        {
            NamespacesDictionary.TryGetValue(ns, out var prefix);

            return $"{prefix?.Name}:{localName}";
        }

        return localName;
    }

    public static string GetNamespace(string? prefix)
    {
        if (prefix != null)
        {
            PrefixDictionary.TryGetValue(prefix, out var qName);

            return qName?.Namespace ?? string.Empty;
        }

        return string.Empty;
    }
}