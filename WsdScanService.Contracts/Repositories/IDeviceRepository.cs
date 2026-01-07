using WsdScanService.Contracts.Entities;

namespace WsdScanService.Contracts.Repositories;

public interface IDeviceRepository
{
    void Add(Device device);

    Device? GetById(string? deviceId);

    Device? GetByAddress(string? address);

    bool RemoveById(string deviceId);

    bool RemoveByAddress(string address);

    ICollection<Device> ToCollection();
}