namespace WsdScanService.Contracts.Scanner.Entities;

public record ScanDestination
{
    public required string DisplayName { get; init; }
    
    public required string Id {get; init; }
}
