namespace WsdScanService.Contracts.Scanner.Entities;

public class ScanDeviceMetadata
{
    public required string ModelName { get; set; }

    public required string SerialNumber { get; set; }

    public required string ScanServiceAddress { get; set; }

    public override string ToString()
    {
        return $"Model: {ModelName}, Serial: {SerialNumber}, ScanService: {ScanServiceAddress}";
    }
}
