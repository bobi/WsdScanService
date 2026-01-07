namespace WsdScanService.Discovery.Configuration;

internal class DiscoveryConfiguration
{
    public const string WsDiscovery = "WsdScanService.Discovery";

    public bool IpV6 { get; set; }

    public uint InstanceId { get; set; } = Convert.ToUInt32(DateTimeOffset.Now.ToUnixTimeSeconds());

    public TimeSpan ProbeInitialDelay { get; set; } = TimeSpan.FromSeconds(2);

    public TimeSpan ProbeRepeatDelay { get; set; } = TimeSpan.FromSeconds(10);

    public int ProbeRepeatTimes { get; set; } = 10;
}