using WsdScanService.Contracts.ScanService;

namespace WsdScanService.Contracts.Entities;

public class Device
{
    public required string DeviceId { get; init; }

    /**
     * Device Ip Address
     */
    public required string Address { get; init; }

    public required string Type { get; init; }
    public required string MexAddress { get; init; }

    public ScanDeviceMetadata? ScanDeviceMetadata { get; set; }

    public Dictionary<string, Subscription> Subscriptions { get; } = new();
    public ScannerConfigurationType? ScannerConfiguration { get; set; }
    public StatusSummaryType? Status { get; set; }

    public ScanTicketType? DefaultScanTicket { get; set; }
}
