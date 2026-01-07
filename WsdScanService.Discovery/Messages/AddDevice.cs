namespace WsdScanService.Discovery.Messages;

public record AddDevice(string DeviceId, string Address, string Type) : IMessage;