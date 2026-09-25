namespace WsdScanService.Common.Configuration;

public record WsdConfiguration
{
    public const int DefaultImageConverterTimeoutSeconds = 180;

    // Max run time for each image converter; the process is killed when exceeded
    public int ImageConverterTimeoutSeconds { get; init; } = DefaultImageConverterTimeoutSeconds;

    // Named converters applied to images from the WSD backend; "default" is used when a profile names none
    public IDictionary<string, ImageConverterConfiguration>? ImageConverters { get; init; }
}
