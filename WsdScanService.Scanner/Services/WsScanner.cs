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
        if (configuration.Value.Sane?.UseSaneBackend ?? false)
        {
            return await saneScanner.CreateScanJobAsync(
                scanServiceAddress,
                scanIdentifier,
                destinationToken,
                scanTicket
            );
        }

        return await wsScanServiceClientService.CreateScanJobAsync(
            scanServiceAddress,
            scanIdentifier,
            destinationToken,
            scanTicket
        );
    }

    public async Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob)
    {
        if (configuration.Value.Sane?.UseSaneBackend ?? false)
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
        if (configuration.Value.Sane?.UseSaneBackend ?? false)
        {
            return await saneScanner.RetrieveImage(scanServiceAddress, scanJob);
        }

        return await wsScanServiceClientService.RetrieveImage(scanServiceAddress, scanJob);
    }

    public async Task GetJobHistoryAsync(string scanServiceAddress)
    {
        await wsScanServiceClientService.GetJobHistoryAsync(scanServiceAddress);
    }

    public async Task GetActiveJobsAsync(string scanServiceAddress)
    {
        await wsScanServiceClientService.GetActiveJobsAsync(scanServiceAddress);
    }
}