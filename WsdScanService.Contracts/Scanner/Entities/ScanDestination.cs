namespace WsdScanService.Contracts.Scanner.Entities;

using ClientContext = string;

public class ScanDestination
{
    public required string DisplayName { get; init; }
    
    public required ClientContext Id {get; init; }
}
