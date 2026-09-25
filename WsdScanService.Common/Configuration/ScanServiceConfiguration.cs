namespace WsdScanService.Common.Configuration;

public record ScanServiceConfiguration
{
    public const string WsdScanService = "WsdScanService";

    public const int DefaultWsdScanTimeoutSeconds = 180;

    public required string Ip { get; set; }

    public int Port { get; init; } = 5000;

    public required string OutputDir { get; init; }

    public required string ScanEndpoint { get; init; } = "/wsd/scan";

    public required IList<ScanProfile> ScanProfiles { get; init; } = new List<ScanProfile>();

    public int SubscriptionRenewInterval { get; init; } = 300; // in seconds

    public int RemovalGracePeriodMs { get; init; } = 10000; // in milliseconds

    public int RenewCheckInterval { get; init; } = 10; // in seconds
    
    public int RenewThreshold { get; init; } = 60; // in seconds

    // Max duration of a single WSD scan service call (e.g. RetrieveImage, which spans the whole scan)
    public int WsdScanTimeoutSeconds { get; init; } = DefaultWsdScanTimeoutSeconds; // in seconds
    
    public SaneConfiguration? Sane { get; init; }

    public WsdConfiguration? Wsd { get; init; }
}