using System.Net;
using WsdScanService.Discovery.Protocol;

namespace WsdScanService.Discovery.Configuration;

public class Configuration
{
    public const string WsDiscovery = "WsdScanService.Discovery";

    private bool _ipV6;

    public bool IpV6
    {
        get => _ipV6;
        set
        {
            _ipV6 = value;

            MulticastAddressEndPoint = value
                ? new IPEndPoint(
                    IPAddress.Parse(ProtocolConstants.MulticastIPv6Address.Host),
                    ProtocolConstants.MulticastIPv6Address.Port
                )
                : new IPEndPoint(
                    IPAddress.Parse(ProtocolConstants.MulticastIPv4Address.Host),
                    ProtocolConstants.MulticastIPv4Address.Port
                );
        }
    }

    public string ServiceId { get; set; } = $"urn:uuid:{Guid.NewGuid()}";

    public int InstanceId { get; set; } = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds());

    public TimeSpan ProbeInitialDelay { get; set; } = TimeSpan.FromSeconds(2);

    public TimeSpan ProbeRepeatDelay { get; set; } = TimeSpan.FromSeconds(10);

    public int ProbeRepeatTimes { get; set; } = -1;

    public required IPEndPoint MulticastAddressEndPoint;
}