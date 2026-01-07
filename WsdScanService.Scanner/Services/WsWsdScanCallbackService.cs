using CoreWCF;
using CoreWCF.Channels;
using Microsoft.Extensions.Logging;
using WsdScanService.Common.Extensions;
using WsdScanService.Contracts;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Contracts.ScanService;

namespace WsdScanService.Scanner.Services;

internal class WsWsdScanCallbackService(
    ILogger<WsWsdScanCallbackService> logger,
    ScanJobManager scanJobManager,
    IDeviceRepository deviceRepository) : IWsWsdScannerServiceCallback
{
    private RemoteEndpointMessageProperty? GetRemoteEndpoint()
    {
        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out var prop))
        {
            return prop as RemoteEndpointMessageProperty;
        }
        return null;
    }

    public async Task ScanAvailableEventAsync(ScanAvailableEvent request)
    {
        var remoteEndpoint = GetRemoteEndpoint();

        logger.LogDebug("ScanAvailableEventAsync: {}", request.DumpAsYaml());
        logger.LogDebug("ScanAvailableEventAsync: {0}", remoteEndpoint?.Address);

        var device = deviceRepository.GetByAddress(remoteEndpoint?.Address);

        if (device is not null)
        {
            await scanJobManager.StartNewJob(device);
        }
        else
        {
            logger.LogWarning("ScanAvailableEventAsync: Device not found for address {}", remoteEndpoint?.Address);
        }
    }

    public Task ScannerElementsChangeEventAsync(ScannerElementsChangeEvent request)
    {
        logger.LogDebug("ScannerElementsChangeEventAsync: {}", request.DumpAsYaml());

        var remoteEndpoint = GetRemoteEndpoint();
        var device = deviceRepository.GetByAddress(remoteEndpoint?.Address);

        if (device != null && request.ScannerElementsChangeEvent1?.ElementChanges != null)
        {
            var changes = request.ScannerElementsChangeEvent1.ElementChanges;
            if (changes.ScannerConfiguration != null)
            {
                device.ScannerConfiguration = changes.ScannerConfiguration;
            }
            if (changes.DefaultScanTicket != null)
            {
                device.DefaultScanTicket = changes.DefaultScanTicket;
            }
        }

        return Task.CompletedTask;
    }

    public Task ScannerStatusSummaryEventAsync(ScannerStatusSummaryEvent request)
    {
        logger.LogDebug("ScannerStatusSummaryEventAsync: {}", request.DumpAsYaml());

        var remoteEndpoint = GetRemoteEndpoint();
        var device = deviceRepository.GetByAddress(remoteEndpoint?.Address);

        if (device != null && request.ScannerStatusSummaryEvent1?.StatusSummary != null)
        {
            device.Status = request.ScannerStatusSummaryEvent1.StatusSummary;
        }

        return Task.CompletedTask;
    }

    public Task ScannerStatusConditionEventAsync(ScannerStatusConditionEvent request)
    {
        logger.LogDebug("ScannerStatusConditionEventAsync: {}", request.DumpAsYaml());
        // Logic to add conditions to device would go here
        return Task.CompletedTask;
    }

    public Task ScannerStatusConditionClearedEventAsync(ScannerStatusConditionClearedEvent request)
    {
        logger.LogDebug("ScannerStatusConditionClearedEventAsync: {}", request.DumpAsYaml());
        // Logic to remove conditions from device would go here
        return Task.CompletedTask;
    }

    public Task JobStatusEventAsync(JobStatusEvent request)
    {
        logger.LogDebug("JobStatusEventAsync: {}", request.DumpAsYaml());

        return Task.CompletedTask;
    }

    public Task JobEndStateEventAsync(JobEndStateEvent request)
    {
        logger.LogDebug("JobEndStateEventAsync: {}", request.DumpAsYaml());

        var endState = request.JobEndStateEvent1.JobEndState;
        
        if (endState != null)
        {
            scanJobManager.CompleteJob(endState);
        }

        return Task.CompletedTask;
    }
}
