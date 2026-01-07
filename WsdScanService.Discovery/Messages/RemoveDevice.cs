namespace WsdScanService.Discovery.Messages;

public record RemoveDevice(string DeviceId) : IMessage;