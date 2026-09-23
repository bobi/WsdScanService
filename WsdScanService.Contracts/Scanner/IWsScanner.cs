using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Contracts.Scanner;

public interface IWsScanner
{
    public Task<Subscription> SubscribeAsync(
        string scanServiceAddress,
        SubscriptionEventType eventType,
        IEnumerable<ScanDestination> scanDestinations
    );

    public Task UnsubscribeAsync(string scanServiceAddress, string subscriptionIdentifier);

    public Task<DateTime> RenewSubscriptionAsync(string scanServiceAddress, string subscriptionIdentifier);
    
    public Task<ScanDeviceMetadata> GetScanDeviceMetadataAsync(string deviceId, string mexAddress);

    public Task<ScanJob> CreateScanJobAsync(
        string scanServiceAddress,
        string scanIdentifier,
        string destinationToken,
        ScanTicket scanTicket
    );

    public Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob);

    public Task<byte[]?> RetrieveImageAsync(string scanServiceAddress, ScanJob scanJob);
    
    public Task GetJobHistoryAsync(string scanServiceAddress);

    public Task GetActiveJobsAsync(string scanServiceAddress);
}