using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Contracts.Repositories;

public interface IDeviceRepository
{
    void Add(Device device);

    Device GetById(string deviceId);

    bool HasById(string deviceId);

    Device GetByHostAddress(string address);

    bool HasByHostAddress(string address);

    bool RemoveById(string deviceId);

    bool RemoveByAddress(string address);

    ICollection<Device> ToCollection();
}