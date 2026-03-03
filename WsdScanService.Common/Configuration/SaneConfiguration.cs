namespace WsdScanService.Common.Configuration;

public record ImageConverterConfiguration
{
    public string? Path { get; init; }

    public ICollection<string>? Args { get; init; }
}

public record SaneConfiguration
{
    public bool UseSaneBackend { get; init; } = false;

    public required string Device { get; init; }

    public string? BackendPath { get; init; }

    public ICollection<string>? AdditionalArgs { get; init; }

    public string? Format { get; init; }

    public ImageConverterConfiguration? ImageConverter { get; init; }
}