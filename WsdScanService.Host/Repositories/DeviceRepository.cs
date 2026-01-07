using System.Collections.Concurrent;
using System.Net;
using WsdScanService.Contracts.Entities;
using WsdScanService.Contracts.Repositories;

namespace WsdScanService.Host.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly ConcurrentDictionary<string, Device> _devicesById = new();

    private readonly ConcurrentDictionary<string, Device> _devicesByAddress = new();

    public void Add(Device device)
    {
        _devicesById[device.DeviceId] = device;

        var addr = device.Address;

        if (Uri.TryCreate(addr, UriKind.Absolute, out var resultUri))
        {
            _devicesByAddress[resultUri.Host] = device;
        }
        else if (IPAddress.TryParse(addr, out var ipAddr))
        {
            _devicesByAddress[ipAddr.ToString()] = device;
        }
        else
        {
            _devicesByAddress[addr] = device;
        }
    }

    public Device? GetById(string? deviceId)
    {
        return deviceId is null ? null : _devicesById.GetValueOrDefault(deviceId);
    }

    public Device? GetByAddress(string? address)
    {
        return address is null ? null : _devicesByAddress.GetValueOrDefault(address);
    }

    public bool RemoveById(string deviceId)
    {
        var remove = _devicesById.Remove(deviceId, out var device);

        if (remove && device != null)
        {
            remove &= _devicesByAddress.Remove(device.Address, out _);
        }

        return remove;
    }

    public bool RemoveByAddress(string address)
    {
        var remove = _devicesByAddress.Remove(address, out var device);

        if (remove && device != null)
        {
            remove &= _devicesById.Remove(device.DeviceId, out _);
        }

        return remove;
    }

    public ICollection<Device> ToCollection()
    {
        return _devicesById.Values.ToList();
    }
}