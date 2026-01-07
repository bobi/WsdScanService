namespace WsdScanService.Common.Configuration;

public class WsdScanServiceHostConfiguration
{
    public const string WsdScanServiceHost = "WsdScanService.Host";

    public required string Ip { get; set; }

    public int Port { get; set; } = 5000;

    public required string ScanEndpoint { get; set; }
    public required string ScanDisplayName { get; set; }
    public required string ScanClientContext { get; set; }
    
    public int SubscriptionRenewInterval { get; set; } = 3600; // in seconds, default is 1 hour
}
