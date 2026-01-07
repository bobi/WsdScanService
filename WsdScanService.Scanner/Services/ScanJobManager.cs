using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Extensions;
using WsdScanService.Contracts;
using WsdScanService.Contracts.Entities;
using WsdScanService.Contracts.ScanService;
using WsdScanService.Scanner.Utils;

namespace WsdScanService.Scanner.Services
{
    internal class ScanJobManager(
        ILogger<ScanJobManager> logger,
        IOptions<Configuration.Configuration> configuration,
        WsWsdScanClientService scanClientService)
        : BackgroundService
    {
        private class ScanJob
        {
            public required Device Device { get; init; }
            public required int JobId { get; init; }
            public required string JobToken { get; init; }
            public required int ImagesToTransfer { get; init; }
        }

        private readonly BlockingCollection<string> _newJobs = new();

        private readonly ConcurrentDictionary<string, ScanJob> _scanJobs = new();

        public async Task AddJob(Device device)
        {
            var createScanJobResponse = await scanClientService.CreateScanJobAsync(device);

            var jobId = createScanJobResponse.CreateScanJobResponse1.JobId.Value;
            var jobToken = createScanJobResponse.CreateScanJobResponse1.JobToken.Value;
            var jobKey = $"{device.DeviceId}-{jobId}-{jobToken}";

            var scanJob = new ScanJob
            {
                Device = device,
                JobId = jobId,
                JobToken = jobToken,
                ImagesToTransfer = createScanJobResponse.CreateScanJobResponse1.DocumentFinalParameters.ImagesToTransfer
                    .Value
            };

            if (_scanJobs.TryAdd(jobKey, scanJob))
            {
                _newJobs.Add(jobKey);

                logger.LogInformation("Added new scan job to process: {JobId}, for Device: {Device}", jobId,
                    device.DeviceId);
            }
            else
            {
                logger.LogWarning("Failed to add job {JobId} because it already exists, for Device: {Device}", jobId,
                    device.DeviceId);
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
                        catch (Exception ex)
                        {
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

            logger.LogDebug("Scan job processor loop complete");
        }

        private async Task RetrieveImages(ScanJob scanJob, string outputDir, CancellationToken cancellationToken)
        {
            var wsWsdScanClient =
                new WsWsdScanClient(scanJob.Device.ScanDeviceMetadata!.ScanServiceAddress);

            wsWsdScanClient.AddTraceMessageLogBehavior(logger);
            var retrieveRequest = new RetrieveImageRequest
            {
                RetrieveImageRequest1 = new RetrieveImageRequestType
                {
                    JobId = new IntOneExtType { Value = scanJob.JobId },
                    JobToken = new String255ExtType { Value = scanJob.JobToken }
                }
            };

            var imagesToTransfer = scanJob.ImagesToTransfer;
            while (imagesToTransfer > 0)
            {
                var retrieveResponse = await wsWsdScanClient.RetrieveImageAsync(retrieveRequest);
                var imageData = retrieveResponse.RetrieveImageResponse1.ScanData.Value;
                
                if (imageData is { Length: > 0 })
                {
                    await FileUtils.WriteUniqueFileWithSuffix(
                        Path.Combine(outputDir, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}.jpg"),
                        imageData,
                        cancellationToken
                    );

                    imagesToTransfer--;
                }
            }
        }

        private static async Task CancelJob(ScanJob scanJob)
        {
            var wsWsdScanClient =
                new WsWsdScanClient(scanJob.Device.ScanDeviceMetadata!.ScanServiceAddress);

            var cancelJobRequest = new CancelJobRequest
            {
                CancelJobRequest1 = new CancelJobRequestType
                {
                    JobId = new IntOneExtType { Value = scanJob.JobId },
                }
            };

            await wsWsdScanClient.CancelJobAsync(cancelJobRequest);
        }

        public Task StartNewJob(Device device)
        {
            return Task.Run(() => AddJob(device));
        }

        public void CompleteJob(JobEndStateType jobEndState)
        {
            logger.LogInformation("Completed scan job: {JobState}", jobEndState.DumpAsYaml());
        }
    }
}