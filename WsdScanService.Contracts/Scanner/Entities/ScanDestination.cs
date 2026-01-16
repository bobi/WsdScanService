namespace WsdScanService.Contracts.Scanner.Entities;

using ClientContext = string;

public record ScanDestination
{
    public required string DisplayName { get; init; }
    
    public required ClientContext Id {get; init; }
}
