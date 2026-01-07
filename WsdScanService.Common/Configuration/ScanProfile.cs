namespace WsdScanService.Common.Configuration;

public class ScanProfile
{
    public string Id { get; } = $"{Guid.NewGuid()}";

    public required string DisplayName { get; init; }

    public required int Resolution { get; init; }
}