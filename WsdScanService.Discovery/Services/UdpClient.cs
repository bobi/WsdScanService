using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.Services;

internal class UdpClient : IHostedService
{
    private readonly ILogger<UdpClient> _logger;
    private readonly IPEndPoint _multicastAddressEndPoint;
    private readonly System.Net.Sockets.UdpClient _udpClient;

    public UdpClient(ILogger<UdpClient> logger, IOptions<Configuration.DiscoveryConfiguration> configuration)
    {
        var multicastAddressEndPoint = configuration.Value.IpV6
            ? new IPEndPoint(
                IPAddress.Parse(ProtocolConstants.MulticastIPv6Address.Host),
                ProtocolConstants.MulticastIPv6Address.Port
            )
            : new IPEndPoint(
                IPAddress.Parse(ProtocolConstants.MulticastIPv4Address.Host),
                ProtocolConstants.MulticastIPv4Address.Port
            );

        _logger = logger;
        _multicastAddressEndPoint = multicastAddressEndPoint;
        _udpClient = new(_multicastAddressEndPoint.Port, _multicastAddressEndPoint.AddressFamily);
        _udpClient.Client.SendTimeout = 5000;
    }

    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ctsToken = default)
    {
        return _udpClient.SendAsync(data, _multicastAddressEndPoint, ctsToken);
    }

    public ValueTask<UdpReceiveResult> ReceiveAsync(CancellationToken ctsToken = default)
    {
        return _udpClient.ReceiveAsync(ctsToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _udpClient.JoinMulticastGroup(_multicastAddressEndPoint.Address);

        _logger.LogInformation(
            "Started announcing and listening on {IpAddress}:{Port}",
            _multicastAddressEndPoint.Address,
            _multicastAddressEndPoint.Port
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _udpClient.DropMulticastGroup(_multicastAddressEndPoint.Address);

        return Task.CompletedTask;
    }
}