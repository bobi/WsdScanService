using System.Xml;
using Ws.Devprof;
using Ws.Mex;
using WsdScanService.Common;
using WsdScanService.Common.Extensions;
using WsdScanService.Contracts;
using WsdScanService.Contracts.Entities;

namespace WsdScanService.Host.Services;

public class WsTransferClientService(ILogger<WsTransferClientService> logger)
{
    public async Task<ScanDeviceMetadata> GetScanDeviceMetadataAsync(Device device)
    {
        await using var client = new WsTransferClient(device.MexAddress);

        client.AddTraceMessageLogBehavior(logger);
        var xmlCaptureBehavior = client.AddXmlCaptureBehavior();

        var soapResponse =
            await client.ExecuteInOperationContextAsync(async context =>
            {
                context.OutgoingMessageHeaders.To = new Uri(device.DeviceId);
                return await client.GetAsync();
            });

        var responseXml = xmlCaptureBehavior.Inspector.LastResponseXml;

        var (thisDeviceType, thisModelType, hostedService) = ParseMetadata(soapResponse.Metadata, responseXml);

        return new ScanDeviceMetadata
        {
            ModelName = thisModelType.ModelName.First().Value,
            SerialNumber = thisDeviceType.SerialNumber,
            ScanServiceAddress = hostedService.EndpointReference.First().Address.Value ?? string.Empty
        };
    }

    private static (ThisDeviceType thisDeviceType, ThisModelType thisModelType, Hosted hostedService)
        ParseMetadata(Metadata metadata, XmlDocument? xmlDoc)
    {
        ThisDeviceType? thisDeviceType = null;
        ThisModelType? thisModelType = null;
        Relationship? relationship = null;

        foreach (var metadataSection in metadata.MetadataSection)
        {
            if (Xd.DeviceProfile.DialectThisDevice == metadataSection.Dialect)
            {
                thisDeviceType = metadataSection.Any.Deserialize<ThisDeviceType>();
            }

            if (Xd.DeviceProfile.DialectThisModel == metadataSection.Dialect)
            {
                thisModelType = metadataSection.Any.Deserialize<ThisModelType>();
            }

            if (Xd.DeviceProfile.DialectRelationship == metadataSection.Dialect)
            {
                relationship = metadataSection.Any.Deserialize<Relationship>();
            }
        }

        if (thisDeviceType == null || thisModelType == null ||
            relationship is not { Type: Xd.DeviceProfile.RelationshipTypeHost })
        {
            throw new Exception("Failed to parse metadata sections.");
        }

        var hostedService = relationship.Any.Where(element => element.LocalName == "Hosted")
            .Select(element => element.Deserialize<Hosted>())
            .First(hosted => XmlUtils
                .ParseTypes(hosted.Types,
                    (prefix) => xmlDoc?.GetAllNamespaces().LookupNamespace(prefix) ?? string.Empty)
                ?.Contains(Xd.ScanService.ScannerServiceTypeQName) ?? false
            );

        return (thisDeviceType, thisModelType, hostedService);
    }
}