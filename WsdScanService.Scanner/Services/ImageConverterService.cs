using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Configuration;
using WsdScanService.Scanner.Utils;

namespace WsdScanService.Scanner.Services;

public class ImageConverterService(ILogger<ImageConverterService> logger)
{
    public const string DefaultImageConverter = "default";

    // Runs the named converter (null = "default") from the given set on the image file at inputPath.
    // Takes ownership of inputPath and returns the path of the resulting file (inputPath itself when no converter
    // applies); the caller owns the returned file.
    public async Task<string> TransformAsync(
        string inputPath,
        string? name,
        IDictionary<string, ImageConverterConfiguration>? converters,
        TimeSpan timeout
    )
    {
        var imageConverter = Resolve(name, converters);

        if (string.IsNullOrEmpty(imageConverter?.Path))
        {
            return inputPath;
        }

        var outputPath = Path.Combine(Path.GetTempPath(), $"scan-transform-image-output-{Guid.NewGuid()}");

        try
        {
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

            return outputPath;
        }
        catch
        {
            File.Delete(outputPath);
            throw;
        }
        finally
        {
            File.Delete(inputPath);
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
