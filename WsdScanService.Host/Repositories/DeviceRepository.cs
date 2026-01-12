using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Host.Repositories;

public class DeviceRepository
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

    public bool TryGetById(string deviceId, [NotNullWhen(true)] out Device? device)
    {
        return _devicesById.TryGetValue(deviceId, out device);
    }

    public bool TryGetByHostAddress(string address, [NotNullWhen(true)] out Device? device)
    {
        return _devicesByAddress.TryGetValue(address, out device);
    }

    public bool TryRemoveById(string deviceId, [NotNullWhen(true)] out Device? device)
    {
        var remove = _devicesById.TryRemove(deviceId, out device);

        if (remove && device != null)
        {
            remove &= _devicesByAddress.TryRemove(GetDeviceAddress(device), out _);
        }

        return remove;
    }

    public bool TryRemoveByAddress(string address, [NotNullWhen(true)] out Device? device)
    {
        var remove = _devicesByAddress.TryRemove(address, out device);

        if (remove && device != null)
        {
            remove &= _devicesById.TryRemove(device.DeviceId, out _);
        }

        return remove;
    }

    public ICollection<Device> ToCollection()
    {
        return _devicesById.Values.ToList();
    }
}