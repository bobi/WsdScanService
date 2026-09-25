using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Scanner.Contracts;
using WsdScanService.Scanner.Utils;

namespace WsdScanService.Scanner.Services;

using ScanJobInfo = (ScanTicket ScanTicket, DateTime ExpirationTime);

public class SaneScanner(ILogger<SaneScanner> logger, IOptions<ScanServiceConfiguration> configuration)
    : BackgroundService, ISaneScanner
{
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

        try
        {
            scannedImagePath = await ScanImage(saneDevice, scanServiceAddress, scanJobInfo);

            return await File.ReadAllBytesAsync(scannedImagePath);
        }
        finally
        {
            if (scannedImagePath != null)
            {
                File.Delete(scannedImagePath);
            }

            _scanJobs.TryRemove(scanJob.JobToken, out _);
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
                ProcessRunner.ReplaceNamedParameters(
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
            WorkingDirectory = Path.GetTempPath()
        };

        foreach (var arg in configuration.Value.Sane?.AdditionalArgs ?? [])
        {
            info.ArgumentList.Add(arg);
        }

        try
        {
            await ProcessRunner.RunAsync(info, "Sane", Timeout, logger);
        }
        catch
        {
            File.Delete(outputPath);
            throw;
        }

        return outputPath;
    }

    private TimeSpan Timeout =>
        TimeSpan.FromSeconds(configuration.Value.Sane?.TimeoutSeconds ?? SaneConfiguration.DefaultTimeoutSeconds);

    private static string GetDeviceAddress(string address)
    {
        if (Uri.TryCreate(address, UriKind.Absolute, out var resultUri))
        {
            return resultUri.Host;
        }

        return IPAddress.TryParse(address, out var ipAddr) ? ipAddr.ToString() : address;
    }
}
