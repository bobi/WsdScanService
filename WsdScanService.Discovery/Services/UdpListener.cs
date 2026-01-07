using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Discovery.Services;

public class UdpListener(
    ILogger<UdpListener> logger,
    UdpClient udpClient,
    DiscoveryPubSub<UdpReceiveResult> pubSub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ctsToken)
    {
        while (!ctsToken.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(ctsToken);

                await pubSub.PublishAsync(result, ctsToken);
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