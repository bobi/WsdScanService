namespace WsdScanService.Discovery.Services.SoapMessageActions;

internal interface ISoapActionHandler
{
    Task HandleAsync(ReadOnlyMemory<byte> data);
}