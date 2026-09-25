using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Configuration;
using WsdScanService.Scanner.Utils;

namespace WsdScanService.Scanner.Services;

public class ImageConverterService(ILogger<ImageConverterService> logger)
{
    public const string DefaultImageConverter = "default";

    // Runs the named converter (null = "default") from the given set; returns the image unchanged when none applies
    public async Task<byte[]> TransformAsync(
        byte[] image,
        string? name,
        IDictionary<string, ImageConverterConfiguration>? converters,
        TimeSpan timeout
    )
    {
        var imageConverter = Resolve(name, converters);

        if (string.IsNullOrEmpty(imageConverter?.Path))
        {
            return image;
        }

        var inputPath = Path.Combine(Path.GetTempPath(), $"scan-transform-image-input-{Guid.NewGuid()}");
        var outputPath = Path.Combine(Path.GetTempPath(), $"scan-transform-image-output-{Guid.NewGuid()}");

        try
        {
            await File.WriteAllBytesAsync(inputPath, image);

            var info = new ProcessStartInfo
            {
                FileName = imageConverter.Path,
                WorkingDirectory = Path.GetTempPath()
            };

            var namedParameters = new Dictionary<string, string>
            {
                { "InputPath", inputPath },
                { "OutputPath", outputPath }
            };

            foreach (var arg in imageConverter.Args ?? [])
            {
                info.ArgumentList.Add(ProcessRunner.ReplaceNamedParameters(arg, namedParameters));
            }

            await ProcessRunner.RunAsync(info, "ImageConverter", timeout, logger);

            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            File.Delete(inputPath);
            File.Delete(outputPath);
        }
    }

    private ImageConverterConfiguration? Resolve(
        string? name,
        IDictionary<string, ImageConverterConfiguration>? converters
    )
    {
        var key = string.IsNullOrEmpty(name) ? DefaultImageConverter : name;

        if (converters?.TryGetValue(key, out var converter) ?? false)
        {
            return converter;
        }

        if (key != DefaultImageConverter)
        {
            logger.LogWarning(
                "ImageConverter '{Name}' is not defined for the active scan backend, image is not converted",
                key
            );
        }

        return null;
    }
}
