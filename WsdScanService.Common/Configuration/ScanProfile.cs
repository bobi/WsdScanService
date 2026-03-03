namespace WsdScanService.Common.Configuration;

public record ScanProfile
{
    public string Id { get; } = $"{Guid.NewGuid()}";

    public required string DisplayName { get; init; }

    public required int Resolution { get; init; }
}