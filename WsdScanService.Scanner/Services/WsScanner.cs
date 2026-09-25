using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.Services;

internal class WsScanner(
    WsEventingClientService wsEventingClientService,
    WsTransferClientService wsTransferClientService,
    WsScanClientService wsScanServiceClientService,
    ISaneScanner saneScanner,
    ImageConverterService imageConverterService,
    IOptions<ScanServiceConfiguration> configuration
) : IWsScanner
{
    public async Task<Subscription> SubscribeAsync(
        string scanServiceAddress,
        SubscriptionEventType eventType,
        IEnumerable<ScanDestination> scanDestinations
    )
    {
        return await wsEventingClientService.SubscribeAsync(scanServiceAddress, eventType, scanDestinations);
    }

    public async Task<DateTime> RenewSubscriptionAsync(string scanServiceAddress, string subscriptionIdentifier)
    {
        return await wsEventingClientService.RenewSubscriptionAsync(scanServiceAddress, subscriptionIdentifier);
    }

    public async Task UnsubscribeAsync(string scanServiceAddress, string subscriptionIdentifier)
    {
        await wsEventingClientService.UnsubscribeAsync(scanServiceAddress, subscriptionIdentifier);
    }

    public async Task<ScanDeviceMetadata> GetScanDeviceMetadataAsync(string deviceId, string mexAddress)
    {
        return await wsTransferClientService.GetScanDeviceMetadataAsync(deviceId, mexAddress);
    }

    public async Task<ScanJob> CreateScanJobAsync(
        string scanServiceAddress,
        string scanIdentifier,
        string destinationToken,
        ScanTicket scanTicket
    )
    {
        var scanJob = UseSaneBackend
            ? await saneScanner.CreateScanJobAsync(scanTicket)
            : await wsScanServiceClientService.CreateScanJobAsync(
                scanServiceAddress,
                scanIdentifier,
                destinationToken,
                scanTicket
            );

        return scanJob with { ImageConverter = scanTicket.ImageConverter };
    }

    public async Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob)
    {
        if (UseSaneBackend)
        {
            await saneScanner.CancelScanJobAsync(scanServiceAddress, scanJob);
        }
        else
        {
            await wsScanServiceClientService.CancelScanJobAsync(scanServiceAddress, scanJob);
        }
    }

    public async Task<byte[]?> RetrieveImageAsync(string scanServiceAddress, ScanJob scanJob)
    {
        var sane = configuration.Value.Sane;
        var wsd = configuration.Value.Wsd;

        var imageData = UseSaneBackend
            ? await saneScanner.RetrieveImage(scanServiceAddress, scanJob)
            : await wsScanServiceClientService.RetrieveImage(scanServiceAddress, scanJob);

        if (imageData is not { Length: > 0 })
        {
            return imageData;
        }

        var (converters, timeoutSeconds) = UseSaneBackend
            ? (sane?.ImageConverters, sane?.TimeoutSeconds ?? SaneConfiguration.DefaultTimeoutSeconds)
            : (wsd?.ImageConverters, wsd?.TimeoutSeconds ?? WsdConfiguration.DefaultTimeoutSeconds);

        return await imageConverterService.TransformAsync(
            imageData,
            scanJob.ImageConverter,
            converters,
            TimeSpan.FromSeconds(timeoutSeconds)
        );
    }

    private bool UseSaneBackend => configuration.Value.Sane?.UseSaneBackend ?? false;

    public async Task GetJobHistoryAsync(string scanServiceAddress)
    {
        await wsScanServiceClientService.GetJobHistoryAsync(scanServiceAddress);
    }

    public async Task GetActiveJobsAsync(string scanServiceAddress)
    {
        await wsScanServiceClientService.GetActiveJobsAsync(scanServiceAddress);
    }
}