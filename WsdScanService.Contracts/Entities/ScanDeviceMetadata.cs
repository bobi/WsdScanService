namespace WsdScanService.Contracts.Entities;

public class ScanDeviceMetadata
{
    public required string ModelName { get; init; }

    public required string SerialNumber { get; init; }

    public required string ScanServiceAddress { get; init; }

    public override string ToString()
    {
        return $"Model: {ModelName}, Serial: {SerialNumber}, ScanService: {ScanServiceAddress}";
    }
}