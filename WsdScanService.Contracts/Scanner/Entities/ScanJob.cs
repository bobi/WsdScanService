namespace WsdScanService.Contracts.Scanner.Entities;

public record ScanJob
{
    public required int JobId { get; init; }
    public required string JobToken { get; init; }
    
    public required int ImagesToTransfer { get; init; }

    // Copied from ScanTicket.ImageConverter, applied to each retrieved image
    public string? ImageConverter { get; init; }
}