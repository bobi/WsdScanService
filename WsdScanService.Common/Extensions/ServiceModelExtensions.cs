using System.ServiceModel.Channels;
using System.Xml;

namespace WsdScanService.Common.Extensions;

public static class ServiceModelExtensions
{
    public static XmlDocument ToXmlDocument(this Message message)
    {
        using var memoryStream = new MemoryStream();

        using var writer = XmlWriter.Create(memoryStream);
        
        message.WriteMessage(writer);
        writer.Flush();
        memoryStream.Position = 0;

        var xmlDocument = new XmlDocument();

        xmlDocument.Load(memoryStream);

        return xmlDocument;
    }
}