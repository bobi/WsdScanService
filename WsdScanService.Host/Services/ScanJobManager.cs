using System.Threading.Channels;
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
    private readonly Channel<ScanJobInfo> _newJobs = Channel.CreateUnbounded<ScanJobInfo>();

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

        _newJobs.Writer.TryWrite(new ScanJobInfo { Device = device, ScanJob = scanJob });

        logger.LogInformation(
            "Added new scan job to process: {JobId}, for Device: {Device}",
            scanJob.JobId,
            device.DeviceId
        );
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting scan job processor loop");

        try
        {
            await foreach (var scanJob in _newJobs.Reader.ReadAllAsync(cancellationToken))
            {
                await ProcessJob(scanJob, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }

        logger.LogDebug("Scan job processor loop completed");
    }

    private async Task ProcessJob(ScanJobInfo scanJob, CancellationToken cancellationToken)
    {
        var jobId = scanJob.ScanJob.JobId;

        logger.LogInformation("Processing job {JobId}", jobId);

        try
        {
            await RetrieveImages(scanJob, configuration.Value.OutputDir, cancellationToken);

            logger.LogInformation("Successfully retrieved images for job {JobId}", jobId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve image for job {JobId}", jobId);

            try
            {
                await CancelJob(scanJob);
            }
            catch (Exception cancelEx)
            {
                logger.LogWarning(cancelEx, "Failed to cancel job {JobId}", jobId);
            }
        }
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

            if (imageData is not { Length: > 0 })
            {
                throw new InvalidOperationException(
                    $"Scanner returned no image data, {imagesToTransfer} image(s) not transferred"
                );
            }

            await FileUtils.WriteUniqueFileWithSuffix(
                Path.Combine(outputDir, $"{DateTime.Now:yyyy-MM-dd_HHmmss}.jpg"),
                imageData,
                cancellationToken
            );

            imagesToTransfer--;
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