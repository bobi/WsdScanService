using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Host.Repositories;

public class DeviceRepository
{
    private readonly ConcurrentDictionary<string, Device> _devicesById = new();

    private readonly ConcurrentDictionary<string, Device> _devicesByAddress = new();

    private readonly Lock _writeLock = new();

    public void Add(Device device)
    {
        lock (_writeLock)
        {
            _devicesById[device.DeviceId] = device;
            _devicesByAddress[GetDeviceAddress(device)] = device;
        }
    }

    public void Update(Device device)
    {
        lock (_writeLock)
        {
            _devicesById.TryGetValue(device.DeviceId, out var oldDevice);
            _devicesById[device.DeviceId] = device;

            var newAddress = GetDeviceAddress(device);
            _devicesByAddress[newAddress] = device;

            if (oldDevice != null)
            {
                var oldAddress = GetDeviceAddress(oldDevice);
                if (oldAddress != newAddress)
                {
                    _devicesByAddress.TryRemove(oldAddress, out _);
                }
            }
        }
    }

    public void UpdateAtomic(string deviceId, Func<Device, Device> updateFactory)
    {
        lock (_writeLock)
        {
            if (_devicesById.TryGetValue(deviceId, out var device))
            {
                var newDevice = updateFactory(device);
                Update(newDevice);
            }
        }
    }

    private static string GetDeviceAddress(Device device)
    {
        var addr = device.MexAddress;

        if (Uri.TryCreate(addr, UriKind.Absolute, out var resultUri))
        {
            return resultUri.Host;
        }

        return IPAddress.TryParse(addr, out var ipAddr) ? ipAddr.ToString() : addr;
    }

    public bool TryGetById(string deviceId, [NotNullWhen(true)] out Device? device)
    {
        lock (_writeLock)
        {
            return _devicesById.TryGetValue(deviceId, out device);
        }
    }

    public bool TryGetByHostAddress(string address, [NotNullWhen(true)] out Device? device)
    {
        return _devicesByAddress.TryGetValue(address, out device);
    }

    public bool TryRemoveById(string deviceId, [NotNullWhen(true)] out Device? device)
    {
        lock (_writeLock)
        {
            var remove = _devicesById.TryRemove(deviceId, out device);

            if (remove && device != null)
            {
                remove &= _devicesByAddress.TryRemove(GetDeviceAddress(device), out _);
            }

            return remove;
        }
    }

    public bool TryRemoveByAddress(string address, [NotNullWhen(true)] out Device? device)
    {
        lock (_writeLock)
        {
            var remove = _devicesByAddress.TryRemove(address, out device);

            if (remove && device != null)
            {
                remove &= _devicesById.TryRemove(device.DeviceId, out _);
            }

            return remove;
        }
    }

    public IImmutableList<Device> ToImmutableList()
    {
        lock (_writeLock)
        {
            return _devicesById.Values.ToImmutableList();
        }
    }
}