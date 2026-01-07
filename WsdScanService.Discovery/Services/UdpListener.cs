using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Discovery.Services;

internal class UdpListener(
    ILogger<UdpListener> logger,
    UdpClient udpClient,
    IUdpMessageProcessor messageProcessor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ctsToken)
    {
        while (!ctsToken.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(ctsToken);

                messageProcessor.ProcessMessage(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}