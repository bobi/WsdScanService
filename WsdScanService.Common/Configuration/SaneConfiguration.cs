namespace WsdScanService.Common.Configuration;

public record ImageConverterConfiguration
{
    public string? Path { get; init; }

    public ICollection<string>? Args { get; init; }

    // Extension (letters/digits, no dot) of the saved file, e.g. "pnm" for a raw converter without Path; null = "jpg"
    public string? Extension { get; init; }
}

public record SaneConfiguration
{
    public const int DefaultTimeoutSeconds = 180;

    public bool UseSaneBackend { get; init; } = false;

    public required string Device { get; init; }

    public string? BackendPath { get; init; }

    public ICollection<string>? AdditionalArgs { get; init; }

    public string? Format { get; init; }

    // Max run time for scanimage and each image converter; the process is killed when exceeded
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;

    // Named converters applied to images from the SANE backend; "default" is used when a profile names none
    public IDictionary<string, ImageConverterConfiguration>? ImageConverters { get; init; }
}