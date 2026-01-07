using System.Net.Sockets;
using Microsoft.Extensions.Hosting;

namespace WsdScanService.Discovery.Services;

public interface IUdpMessageProcessor: IHostedService
{
    public void ProcessMessage(UdpReceiveResult udpReceiveResult);
}