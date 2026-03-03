using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.Services;

using ScanJobInfo = (ScanTicket ScanTicket, DateTime ExpirationTime);

public class SaneScanner(ILogger<SaneScanner> logger, IOptions<ScanServiceConfiguration> configuration)
    : BackgroundService, ISaneScanner
{
    private class InvalidExitCodeException(string? message) : Exception(message);

    private const uint ExpirationTime = 5; //minutes

    private readonly IDictionary<string, string> _formatMap = new Dictionary<string, string>
    {
        { "png", "png" },
        { "jfif", "jpeg" },
        { "tiff", "tiff" }
    };

    private readonly Dictionary<string, string> _sourceMap = new()
    {
        { "Platen", "Flatbed" }
    };

    private readonly Dictionary<string, string> _modeMap = new()
    {
        { "Photo", "Color" }
    };

    private readonly ConcurrentDictionary<string, ScanJobInfo> _scanJobs = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var job in _scanJobs)
                {
                    if (job.Value.ExpirationTime <= DateTime.UtcNow)
                    {
                        _scanJobs.TryRemove(job.Key, out _);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public Task<ScanJob> CreateScanJobAsync(
        ScanTicket scanTicket
    )
    {
        var jobToken = Guid.NewGuid().ToString();

        _scanJobs.TryAdd(jobToken, (scanTicket, DateTime.UtcNow.AddMinutes(ExpirationTime)));

        return Task.FromResult(
            new ScanJob
            {
                JobId = Convert.ToInt32(Environment.TickCount),
                JobToken = jobToken,
                ImagesToTransfer = 1
            }
        );
    }

    public Task CancelScanJobAsync(string scanServiceAddress, ScanJob scanJob)
    {
        _scanJobs.TryRemove(scanJob.JobToken, out _);

        return Task.CompletedTask;
    }

    public async Task<byte[]?> RetrieveImage(string scanServiceAddress, ScanJob scanJob)
    {
        var saneDevice = configuration.Value.Sane?.Device;

        if (string.IsNullOrEmpty(saneDevice))
        {
            throw new InvalidOperationException("Sane device is not configured");
        }

        if (!_scanJobs.TryGetValue(scanJob.JobToken, out var scanJobInfo))
        {
            throw new InvalidOperationException("Scan job not found");
        }

        string? scannedImagePath = null;
        string? transformedImagePath = null;

        try
        {
            scannedImagePath = await ScanImage(saneDevice, scanServiceAddress, scanJobInfo);
            transformedImagePath = await TransformImage(scannedImagePath);

            var imageData = await File.ReadAllBytesAsync(transformedImagePath);

            return imageData;
        }
        catch (InvalidExitCodeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            if (scannedImagePath != null && File.Exists(scannedImagePath))
            {
                File.Delete(scannedImagePath);
            }

            if (transformedImagePath != null && scannedImagePath != transformedImagePath &&
                File.Exists(transformedImagePath))
            {
                File.Delete(transformedImagePath);
            }

            _scanJobs.TryRemove(scanJob.JobToken, out _);
        }

        throw new InvalidOperationException();
    }

    private async Task<string> TransformImage(string inputImagePath)
    {
        if (string.IsNullOrEmpty(configuration.Value.Sane?.ImageConverter?.Path))
        {
            return inputImagePath;
        }

        var outputPath = Path.Combine(Path.GetTempPath(), $"scan-transform-image-output-{Guid.NewGuid()}");

        var info = new ProcessStartInfo
        {
            FileName = configuration.Value.Sane.ImageConverter.Path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetTempPath()
        };

        var namedParameters = new Dictionary<string, string>
        {
            { "InputPath", inputImagePath },
            { "OutputPath", outputPath }
        };

        foreach (var arg in configuration.Value.Sane?.ImageConverter?.Args ?? [])
        {
            info.ArgumentList.Add(ReplaceNamedParameters(arg, namedParameters));
        }

        logger.LogDebug(
            "Running ImageConverter: {FileName} {Arguments}",
            info.FileName,
            string.Join(" ", info.ArgumentList)
        );

        using var process = new Process();

        process.StartInfo = info;

        var lockObject = new object();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (lockObject)
                {
                    logger.LogInformation("STDOUT: {EData}", e.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (lockObject)
                {
                    logger.LogError("STDERR: {EData}", e.Data);
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        logger.LogDebug("ImageConverter Exit code: {ProcessExitCode}", process.ExitCode);

        if (process.ExitCode == 0)
        {
            return outputPath;
        }
        else
        {
            throw new InvalidExitCodeException($"ImageConverter exit code: {process.ExitCode}");
        }
    }

    private async Task<string> ScanImage(string saneDevice, string scanServiceAddress, ScanJobInfo scanJobInfo)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"scan-image-output-{Guid.NewGuid()}");

        var info = new ProcessStartInfo
        {
            FileName = configuration.Value.Sane?.BackendPath ?? "scanimage",
            ArgumentList =
            {
                "--device",
                ReplaceNamedParameters(
                    saneDevice,
                    new Dictionary<string, string>
                    {
                        { "Ip", GetDeviceAddress(scanServiceAddress) }
                    }
                ),
                "--resolution",
                $"{scanJobInfo.ScanTicket.Resolution}dpi",
                "--mode",
                _modeMap[scanJobInfo.ScanTicket.ContentType],
                "--source",
                _sourceMap[scanJobInfo.ScanTicket.InputSource],
                "--format",
                configuration.Value.Sane?.Format ?? _formatMap[scanJobInfo.ScanTicket.Format],
                "--output-file",
                outputPath
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetTempPath()
        };

        foreach (var arg in configuration.Value.Sane?.AdditionalArgs ?? [])
        {
            info.ArgumentList.Add(arg);
        }

        logger.LogDebug("Running sane: {FileName} {Arguments}", info.FileName, string.Join(" ", info.ArgumentList));

        using var process = new Process();

        process.StartInfo = info;

        var lockObject = new object();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (lockObject)
                {
                    logger.LogInformation("STDOUT: {EData}", e.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (lockObject)
                {
                    logger.LogError("STDERR: {EData}", e.Data);
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        logger.LogDebug("Sane Exit code: {ProcessExitCode}", process.ExitCode);

        if (process.ExitCode == 0)
        {
            return outputPath;
        }
        else
        {
            throw new InvalidExitCodeException($"Sane exit code: {process.ExitCode}");
        }
    }


    private static string GetDeviceAddress(string address)
    {
        if (Uri.TryCreate(address, UriKind.Absolute, out var resultUri))
        {
            return resultUri.Host;
        }

        return IPAddress.TryParse(address, out var ipAddr) ? ipAddr.ToString() : address;
    }

    public static string ReplaceNamedParameters(string template, Dictionary<string, string> parameters)
    {
        return parameters.Aggregate(template, (current, kvp) => current.Replace("{" + kvp.Key + "}", kvp.Value));
    }
}