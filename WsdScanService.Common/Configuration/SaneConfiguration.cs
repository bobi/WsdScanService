namespace WsdScanService.Common.Configuration;

public class SaneConfiguration
{
    public bool UseSaneBackend { get; init; } = false;
    public required string Device { get; init; }
    public string? BackendPath { get; init; }
    public ICollection<string>? AdditionalArgs { get; init; }
}