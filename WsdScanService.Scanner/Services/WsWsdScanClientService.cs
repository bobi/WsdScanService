using Microsoft.Extensions.Logging;
using WsdScanService.Common;
using WsdScanService.Common.Extensions;
using WsdScanService.Contracts;
using WsdScanService.Contracts.Entities;
using WsdScanService.Contracts.ScanService;

namespace WsdScanService.Scanner.Services;

public class WsWsdScanClientService(
    ILogger<WsWsdScanClientService> logger)
{
    //TODO: optimize by calling this once and caching the result
    public async Task<T> GetScannerElementsAsync<T>(string url,
        Xd.ScanService.GetScannerElementsRequestedElements requestedElements)
    {
        var scannerElementDataTypes = await GetScannerElementsAsync(
            url,
            requestedElements
        );

        if (scannerElementDataTypes.Length > 0)
        {
            var scannerElementDataType = scannerElementDataTypes.First();
            if (scannerElementDataType.Name == requestedElements.ToXmlQualifiedName() &&
                scannerElementDataType.Item is T scanTicketType)
            {
                return scanTicketType;
            }
        }

        throw new Exception($"No Default Scan Ticket found for: {url}");
    }

    private async Task<ScannerElementDataType[]> GetScannerElementsAsync(
        string url,
        Xd.ScanService.GetScannerElementsRequestedElements requestedElements
    )
    {
        var wsWsdScanClient = new WsWsdScanClient(url);

        wsWsdScanClient.AddTraceMessageLogBehavior(logger);

        var request = new GetScannerElementsRequest
        {
            GetScannerElementsRequest1 = new GetScannerElementsRequestType
            {
                RequestedElements = new RequestedScannerElementsType
                {
                    Name = [requestedElements.ToXmlQualifiedName()]
                }
            }
        };

        var soapResponse = await wsWsdScanClient.GetScannerElementsAsync(request);

        var scannerElementsElementData = soapResponse.GetScannerElementsResponse1.ScannerElements.ElementData;
        return scannerElementsElementData;
    }


    public async Task<CreateScanJobResponse> CreateScanJobAsync(Device device)
    {
        var subscription = device.Subscriptions[Xd.ScanService.CallbackActions.ScanAvailableEvent];

        if (subscription == null) throw new Exception($"No subscription found for device: {device.DeviceId}");

        var wsWsdScanClient = new WsWsdScanClient(device.ScanDeviceMetadata!.ScanServiceAddress);

        wsWsdScanClient.AddTraceMessageLogBehavior(logger);

        var request = new CreateScanJobRequest
        {
            CreateScanJobRequest1 = new CreateScanJobRequestType
            {
                ScanIdentifier = new String255ExtType { Value = subscription.Identifier },
                DestinationToken = new String255ExtType { Value = subscription.DestinationToken },
                ScanTicket = new ScanTicketType
                {
                    JobDescription = new JobDescriptionType
                        { JobName = new String255ExtType { Value = "Scan Job" } },

                    DocumentParameters = new DocumentParametersType
                    {
                        Format = new DocumentFormatType { Value = "jfif" },
                        CompressionQualityFactor = new CompressionQualityFactorType { Value = 100 },
                        InputSource = new DocumentInputSourceType { Value = "Platen" },
                        ContentType = new ContentTypeTicketType { Value = "Photo" },
                        InputSize = new DocumentInputSizeType { Item = new BoolExtType { Value = true } },
                        ImagesToTransfer = new ImagesToTransferType { Value = 1 },
                        MediaSides = new MediaSidesType
                        {
                            MediaFront = new MediaSideType
                            {
                                Resolution = new ResolutionPairExtType
                                {
                                    Width = new IntOneExtType { Value = 600 },
                                    Height = new IntOneExtType { Value = 600 },
                                }
                            }
                        }
                        // Exposure = new ScanExposureType { Item = new BoolExtType { Value = true } },
                        // Scaling = new ScalingType
                        // {
                        //     ScalingWidth = new ScalingRangeType { Value = 100 },
                        //     ScalingHeight = new ScalingRangeType { Value = 100 }
                        // },
                        // Rotation = new RotationType { Value = "0" }
                    }
                }
            }
        };

        var scanJobResponse = await wsWsdScanClient.CreateScanJobAsync(request);

        logger.LogInformation("CreateScanJobAsync Response: {SubscribeResponse}", scanJobResponse.DumpAsYaml());

        return scanJobResponse;
    }
}