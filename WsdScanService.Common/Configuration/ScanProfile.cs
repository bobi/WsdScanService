namespace WsdScanService.Common.Configuration;

public record ScanProfile
{
    public string Id { get; } = $"{Guid.NewGuid()}";

    public required string DisplayName { get; init; }

    public required int Resolution { get; init; }

    // Name of a Sane.ImageConverters entry overriding Sane.ImageConverter (e.g. "autocrop")
    public string? ImageConverter { get; init; }
}