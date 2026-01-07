using Microsoft.Extensions.Hosting;

namespace WsdScanService.Contracts.Discovery;

public interface IDeviceManager: IHostedService
{
    public Task AddDevice(string deviceId, string mexAddress, string type, uint instanceId, uint metadataVersion);

    public Task RemoveDevice(string deviceId, uint instanceId, uint metadataVersion = 0);
}