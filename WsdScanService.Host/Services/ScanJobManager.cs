using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Host.Repositories;
using WsdScanService.Host.Utils;

namespace WsdScanService.Host.Services;

internal class ScanJobManager(
    ILogger<ScanJobManager> logger,
    IOptions<ScanServiceConfiguration> configuration,
    DeviceRepository deviceRepository,
    IWsScanner scanner)
    : BackgroundService, IScanJobManager
{
    private readonly BlockingCollection<string> _newJobs = new();

    private readonly ConcurrentDictionary<string, ScanJobInfo> _scanJobs = new();

    private class ScanJobInfo
    {
        public required Device Device { get; init; }

        public required ScanJob ScanJob { get; init; }
    }

    private async Task AddJob(Device device, string clientContext, string scanIdentifier, string? inputSource)
    {
        var scanJob = await scanner.CreateScanJobAsync(
            device.ScanServiceAddress,
            scanIdentifier,
            device.Subscriptions[SubscriptionEventType.ScanAvailableEvent].DestinationTokens[clientContext],
            new ScanTicket(device.ScanTickets[clientContext])
            {
                InputSource = inputSource ?? ScanTicket.DefaultScanTicket.InputSource
            }
        );

        var jobId = scanJob.JobId;
        var jobToken = scanJob.JobToken;
        var jobKey = $"{device.DeviceId}-{jobId}-{jobToken}";

        var scanJobInfo = new ScanJobInfo
        {
            Device = device,
            ScanJob = scanJob
        };

        if (_scanJobs.TryAdd(jobKey, scanJobInfo))
        {
            _newJobs.Add(jobKey);

            logger.LogInformation(
                "Added new scan job to process: {JobId}, for Device: {Device}",
                jobId,
                device.DeviceId
            );
        }
        else
        {
            logger.LogWarning(
                "Failed to add job {JobId} because it already exists, for Device: {Device}",
                jobId,
                device.DeviceId
            );
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting scan job processor loop");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var jobId = _newJobs.Take(cancellationToken);

                if (_scanJobs.TryGetValue(jobId, out var scanJob))
                {
                    logger.LogInformation("Processing job {JobId}", jobId);

                    try
                    {
                        var outputDir = configuration.Value.OutputDir;
                        if (string.IsNullOrEmpty(outputDir))
                        {
                            logger.LogWarning("Output directory is not configured. Canceling job {JobId}", jobId);

                            await CancelJob(scanJob);
                        }
                        else
                        {
                            await RetrieveImages(scanJob, outputDir, cancellationToken);

                            logger.LogInformation("Successfully retrieved images for job {JobId}", jobId);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await CancelJob(scanJob);

                        logger.LogError(ex, "Failed to retrieve image for job {JobId}", jobId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                await Task.Delay(500, cancellationToken);
            }
        }

        logger.LogDebug("Scan job processor loop completed");
    }

    private async Task RetrieveImages(ScanJobInfo scanJob, string outputDir, CancellationToken cancellationToken)
    {
        var imagesToTransfer = scanJob.ScanJob.ImagesToTransfer;

        while (imagesToTransfer > 0)
        {
            var imageData = await scanner.RetrieveImageAsync(
                scanJob.Device.ScanServiceAddress,
                scanJob.ScanJob
            );

            if (imageData is { Length: > 0 })
            {
                await FileUtils.WriteUniqueFileWithSuffix(
                    Path.Combine(outputDir, $"{DateTime.Now:yyyy-MM-dd_HHmmss}.jpg"),
                    imageData,
                    cancellationToken
                );

                imagesToTransfer--;
            }
        }
    }

    private async Task CancelJob(ScanJobInfo scanJob)
    {
        await scanner.CancelScanJobAsync(scanJob.Device.ScanServiceAddress, scanJob.ScanJob);
    }

    public void StartNewJob(string deviceAddress, string clientContext, string scanIdentifier, string? inputSource)
    {
        if (deviceRepository.TryGetByHostAddress(deviceAddress, out var device))
        {
            Task.Run(() => AddJob(device, clientContext, scanIdentifier, inputSource));
        }
        else
        {
            logger.LogWarning("Device not found for address {DeviceAddress}", deviceAddress);
        }
    }
}