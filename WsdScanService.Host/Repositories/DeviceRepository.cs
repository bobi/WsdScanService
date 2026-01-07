using System.Collections.Concurrent;
using System.Net;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Host.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly ConcurrentDictionary<string, Device> _devicesById = new();

    private readonly ConcurrentDictionary<string, Device> _devicesByAddress = new();

    public void Add(Device device)
    {
        _devicesById[device.DeviceId] = device;
        _devicesByAddress[GetDeviceAddress(device)] = device;
    }

    private static string GetDeviceAddress(Device device)
    {
        var addr = device.MexAddress;

        if (Uri.TryCreate(addr, UriKind.Absolute, out var resultUri))
        {
            return resultUri.Host;
        }
        else if (IPAddress.TryParse(addr, out var ipAddr))
        {
            return ipAddr.ToString();
        }
        else
        {
            return addr;
        }
    }

    public Device GetById(string deviceId)
    {
        return _devicesById[deviceId];
    }

    public bool HasById(string deviceId)
    {
        return _devicesById.ContainsKey(deviceId);
    }

    public Device GetByHostAddress(string address)
    {
        return _devicesByAddress[address];
    }

    public bool HasByHostAddress(string address)
    {
        return _devicesByAddress.ContainsKey(address);
    }

    public bool RemoveById(string deviceId)
    {
        var remove = _devicesById.Remove(deviceId, out var device);

        if (remove && device != null)
        {
            remove &= _devicesByAddress.Remove(GetDeviceAddress(device), out _);
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