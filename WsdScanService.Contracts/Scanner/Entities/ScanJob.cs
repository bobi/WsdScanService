namespace WsdScanService.Contracts.Scanner.Entities;

public record ScanJob
{
    public required int JobId { get; init; }
    public required string JobToken { get; init; }
    
    public required int ImagesToTransfer { get; init; }
}