namespace WsdScanService.Common.Configuration;

public class ScanServiceConfiguration
{
    public const string WsdScanService = "WsdScanService";

    public required string Ip { get; set; }

    public int Port { get; init; } = 5000;

    public required string OutputDir { get; init; }

    public required string ScanEndpoint { get; init; } = "/wsd/scan";

    public required IList<ScanProfile> ScanProfiles { get; init; } = new List<ScanProfile>();

    public int SubscriptionRenewInterval { get; init; } = 300; // in seconds

    public int RemovalGracePeriodMs { get; init; } = 10000; // in milliseconds

    public int RenewCheckInterval { get; init; } = 10; // in seconds
    
    public int RenewThreshold { get; init; } = 60; // in seconds
    
    public SaneConfiguration? Sane { get; init; }
}