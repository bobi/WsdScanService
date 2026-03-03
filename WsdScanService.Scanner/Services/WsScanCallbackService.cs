using CoreWCF;
using CoreWCF.Channels;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Contracts.ScanService;
using WsdScanService.Scanner.Contracts;

namespace WsdScanService.Scanner.Services;

internal class WsScanCallbackService(IScanJobManager scanJobManager) : IWsScannerCallback
{
    private static RemoteEndpointMessageProperty GetRemoteEndpoint()
    {
        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(
                RemoteEndpointMessageProperty.Name,
                out var prop
            ))
        {
            return prop as RemoteEndpointMessageProperty ??
                   throw new Exception("Remote endpoint property is not valid");
        }

        throw new Exception("Remote endpoint not found");
    }

    public Task ScanAvailableEventAsync(ScanAvailableEvent request)
    {
        var remoteEndpoint = GetRemoteEndpoint();

        scanJobManager.StartNewJob(
            remoteEndpoint.Address,
            request.ScanAvailableEvent1.ClientContext.Value,
            request.ScanAvailableEvent1.ScanIdentifier.Value,
            request.ScanAvailableEvent1.InputSource?.Value
        );

        return Task.CompletedTask;
    }

    public Task ScannerElementsChangeEventAsync(ScannerElementsChangeEvent request)
    {
        return Task.CompletedTask;
    }

    public Task ScannerStatusSummaryEventAsync(ScannerStatusSummaryEvent request)
    {
        return Task.CompletedTask;
    }

    public Task ScannerStatusConditionEventAsync(ScannerStatusConditionEvent request)
    {
        return Task.CompletedTask;
    }

    public Task ScannerStatusConditionClearedEventAsync(ScannerStatusConditionClearedEvent request)
    {
        return Task.CompletedTask;
    }

    public Task JobStatusEventAsync(JobStatusEvent request)
    {
        return Task.CompletedTask;
    }

    public Task JobEndStateEventAsync(JobEndStateEvent request)
    {
        return Task.CompletedTask;
    }
}