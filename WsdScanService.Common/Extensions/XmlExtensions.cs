using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using CommunityToolkit.HighPerformance;

namespace WsdScanService.Common.Extensions;

public static class XmlExtensions
{
    public static T DeserializeFromXml<T>(this byte[] data) where T : class, new()
    {
        return new ReadOnlyMemory<byte>(data).DeserializeFromXml<T>();
    }

    public static T DeserializeFromXml<T>(this ReadOnlyMemory<byte> data) where T : class, new()
    {
        var serializer = new XmlSerializer(typeof(T));


        using var stream = data.AsStream();

        using var xmlReader = XmlReader.Create(stream);

        var obj = serializer.Deserialize(xmlReader);

        if (obj == null)
        {
            throw new NullReferenceException("Unable to deserialize data");
        }

        return (T)obj;
    }

    public static T Deserialize<T>(this XmlNode data) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(data);

        var serializer = new XmlSerializer(typeof(T));


        using var xmlNodeReader = new XmlNodeReader(data);

        var obj = serializer.Deserialize(xmlNodeReader);

        if (obj == null)
        {
            throw new NullReferenceException("Unable to deserialize data");
        }

        return (T)obj;
    }
    
    public static ReadOnlyMemory<byte> SerializeToXml<T>(this T obj, XmlQualifiedName[]? namespaces = null)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var ns = new XmlSerializerNamespaces(namespaces ?? []);

        var serializer = new XmlSerializer(typeof(T));

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

        using var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            OmitXmlDeclaration = true
        });

        serializer.Serialize(xmlWriter, obj, ns);

        return memoryStream.GetBuffer();
    }
    
    public static XmlNamespaceManager GetAllNamespaces(this XmlDocument xDoc)
    {
        var result = new XmlNamespaceManager(xDoc.NameTable);

        var xNav = xDoc.CreateNavigator();
        while (xNav != null && xNav.MoveToFollowing(XPathNodeType.Element))
        {
            var localNamespaces = xNav.GetNamespacesInScope(XmlNamespaceScope.Local);
            foreach (var localNamespace in localNamespaces)
            {
                var prefix = localNamespace.Key;
                if (string.IsNullOrEmpty(prefix))
                    prefix = string.Empty;

                result.AddNamespace(prefix, localNamespace.Value);
            }
        }

        return result;
    }
}