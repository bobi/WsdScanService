namespace WsdScanService.Common.Configuration;

public record WsdConfiguration
{
    public const int DefaultTimeoutSeconds = 180;

    // Max duration of a single WSD scan service call (e.g. RetrieveImage, which spans the whole scan)
    // and max run time for each image converter; the converter process is killed when exceeded
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;

    // Named converters applied to images from the WSD backend; "default" is used when a profile names none
    public IDictionary<string, ImageConverterConfiguration>? ImageConverters { get; init; }
}
