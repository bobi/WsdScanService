using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Contracts.Scanner.Entities;

namespace WsdScanService.Host.Services;

public class DeviceManager(
    ILogger<DeviceManager> logger,
    IOptions<ScanServiceConfiguration> configuration,
    IDeviceRepository deviceRepository,
    IWsScanner scanner) : IDeviceManager
{
    private readonly List<ScanDestination> _scanDestinations = configuration.Value.ScanProfiles.Select(e =>
            new ScanDestination { DisplayName = e.DisplayName, Id = e.Id }
        )
        .ToList();

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _devicesToRemove = new();

    public async Task RemoveDevice(string deviceId, uint instanceId, uint metadataVersion = 0)
    {
        if (!deviceRepository.HasById(deviceId)) return;

        var device = deviceRepository.GetById(deviceId);

        if (instanceId < device.InstanceId)
        {
            logger.LogDebug(
                "Ignoring stale Bye for DeviceId: {DeviceId}. Current InstanceId: {Current}, Bye InstanceId: {Bye}",
                deviceId,
                device.InstanceId,
                instanceId
            );
            return;
        }

        if (metadataVersion > 0 && instanceId == device.InstanceId && metadataVersion < device.MetadataVersion)
        {
            logger.LogDebug(
                "Ignoring stale Bye for DeviceId: {DeviceId}. Current MetadataVersion: {Current}, Bye MetadataVersion: {Bye}",
                deviceId,
                device.MetadataVersion,
                metadataVersion
            );
            return;
        }

        logger.LogDebug("Device marked for removal (grace period started): {DeviceId}", deviceId);

        if (_devicesToRemove.TryGetValue(deviceId, out var tokenSource))
        {
            await tokenSource.CancelAsync();
        }

        var removalTokenSource = new CancellationTokenSource();
        _devicesToRemove[deviceId] = removalTokenSource;
        var token = removalTokenSource.Token;

        try
        {
            await Task.Delay(configuration.Value.RemovalGracePeriodMs, token);

            if (!token.IsCancellationRequested)
            {
                await PerformDeviceRemoval(device);
            }
            else
            {
                logger.LogDebug("Device removal cancelled for {DeviceId}", deviceId);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Device removal cancelled for {DeviceId}", deviceId);
        }
    }

    private async Task PerformDeviceRemoval(Device device)
    {
        try
        {
            foreach (var subscription in device.Subscriptions)
            {
                await scanner.UnsubscribeAsync(device.ScanServiceAddress, subscription.Value.Identifier);
            }

            deviceRepository.RemoveById(device.DeviceId);

            logger.LogInformation("Device removed: {DeviceId}", device.DeviceId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while removing device subscription: {DeviceId}", device.DeviceId);
        }
    }

    public async Task AddDevice(string deviceId, string mexAddress, string type, uint instanceId, uint metadataVersion)
    {
        logger.LogInformation(
            "Device discovery signal: {DeviceId} at {Address}, InstanceId: {InstanceId}, MetadataVersion: {MetadataVersion}",
            deviceId,
            mexAddress,
            instanceId,
            metadataVersion
        );

        if (deviceRepository.HasById(deviceId))
        {
            await UpdateDevice(deviceId, mexAddress, instanceId, metadataVersion);
        }
        else
        {
            await AddNewDevice(deviceId, mexAddress, type, instanceId, metadataVersion);
        }
    }

    private async Task UpdateDevice(string deviceId, string mexAddress, uint instanceId, uint metadataVersion)
    {
        var existingDevice = deviceRepository.GetById(deviceId);

        if (instanceId >= existingDevice.InstanceId)
        {
            if (_devicesToRemove.TryGetValue(deviceId, out var tokenSource) && !tokenSource.IsCancellationRequested)
            {
                logger.LogInformation(
                    "Cancelling pending removal for device {DeviceId} due to fresh Hello/ProbeMatch",
                    deviceId
                );
                await tokenSource.CancelAsync();
                _devicesToRemove.Remove(deviceId, out _);
            }

            if (instanceId > existingDevice.InstanceId || metadataVersion > existingDevice.MetadataVersion)
            {
                logger.LogInformation(
                    "Updating device metadata for {DeviceId}. Old Version: {OldVersion}, New Version: {NewVersion}",
                    deviceId,
                    existingDevice.MetadataVersion,
                    metadataVersion
                );

                try
                {
                    var updatedMetadata = await scanner.GetScanDeviceMetadataAsync(deviceId, mexAddress);

                    existingDevice.ModelName = updatedMetadata.ModelName;
                    existingDevice.SerialNumber = updatedMetadata.SerialNumber;
                    existingDevice.ScanServiceAddress = updatedMetadata.ScanServiceAddress;

                    foreach (var subscriptionEntry in existingDevice.Subscriptions)
                    {
                        try
                        {
                            var subscription = await scanner.SubscribeAsync(
                                existingDevice.ScanServiceAddress,
                                subscriptionEntry.Key,
                                _scanDestinations
                            );

                            existingDevice.Subscriptions[subscriptionEntry.Key] = subscription;

                            logger.LogDebug(
                                "Resubscribed {SubscriptionId} for device {DeviceId}",
                                subscription.Identifier,
                                deviceId
                            );
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Failed to resubscribe {SubscriptionId} for device {DeviceId}.",
                                subscriptionEntry.Value.Identifier,
                                deviceId
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to update device metadata for {DeviceId}", deviceId);
                }

                existingDevice.InstanceId = instanceId;
                existingDevice.MetadataVersion = metadataVersion;
            }
        }
    }

    private async Task AddNewDevice(
        string deviceId,
        string mexAddress,
        string type,
        uint instanceId,
        uint metadataVersion
    )
    {
        try
        {
            var scanDeviceMetadata = await scanner.GetScanDeviceMetadataAsync(deviceId, mexAddress);

            var subscriptions = new Dictionary<SubscriptionEventType, Subscription>
            {
                {
                    SubscriptionEventType.ScanAvailableEvent,
                    await scanner.SubscribeAsync(
                        scanDeviceMetadata.ScanServiceAddress,
                        SubscriptionEventType.ScanAvailableEvent,
                        _scanDestinations
                    )
                }
            };


            var newDevice = new Device
            {
                DeviceId = deviceId,
                Type = type,
                MexAddress = mexAddress,
                ScanServiceAddress = scanDeviceMetadata.ScanServiceAddress,
                ModelName = scanDeviceMetadata.ModelName,
                SerialNumber = scanDeviceMetadata.SerialNumber,
                ScanDestinations = _scanDestinations,
                Subscriptions = subscriptions,
                ScanTickets = configuration.Value.ScanProfiles.ToDictionary(
                    e => e.Id,
                    e => new ScanTicket
                    {
                        Resolution = e.Resolution
                    }
                ),
                InstanceId = instanceId,
                MetadataVersion = metadataVersion
            };

            deviceRepository.Add(newDevice);

            logger.LogInformation("Device added: {Device}", newDevice);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while adding new device: {DeviceId} at {Address}", deviceId, mexAddress);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in deviceRepository.ToCollection())
        {
            await PerformDeviceRemoval(device);
        }
    }
}