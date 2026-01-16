namespace WsdScanService.Discovery.Configuration;

internal record DiscoveryConfiguration
{
    public const string WsDiscovery = "WsdScanService.Discovery";

    public bool IpV6 { get; init; }

    public uint InstanceId { get; init; } = Convert.ToUInt32(DateTimeOffset.Now.ToUnixTimeSeconds());

    public TimeSpan ProbeInitialDelay { get; init; } = TimeSpan.FromSeconds(2);

    public TimeSpan ProbeRepeatDelay { get; init; } = TimeSpan.FromSeconds(30);

    public int ProbeRepeatTimes { get; init; } = 5;
}