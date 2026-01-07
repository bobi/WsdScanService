namespace WsdScanService.Discovery.Services.SoapMessageActions;

public interface ISoapActionHandler
{
    Task HandleAsync(ReadOnlyMemory<byte> data, CancellationToken ctsToken);
}