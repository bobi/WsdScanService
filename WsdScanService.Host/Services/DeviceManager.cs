using System.Collections.Immutable;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Discovery;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Host.Repositories;

namespace WsdScanService.Host.Services;

public class DeviceManager(
    ILogger<DeviceManager> logger,
    IOptions<ScanServiceConfiguration> configuration,
    DeviceRepository deviceRepository,
    DeviceRemovalTracker removalTracker,
    IWsScanner scanner) : IDeviceManager
{
    private readonly List<ScanDestination> _scanDestinations = configuration.Value.ScanProfiles.Select(e =>
            new ScanDestination { DisplayName = e.DisplayName, Id = e.Id }
        )
        .ToList();

    public Task RemoveDevice(string deviceId, uint instanceId, uint metadataVersion = 0)
    {
        if (!deviceRepository.TryGetById(deviceId, out var device))
            return Task.CompletedTask;

        if (instanceId < device.InstanceId)
        {
            logger.LogDebug(
                "Ignoring stale Bye for DeviceId: {DeviceId}. Current InstanceId: {Current}, Bye InstanceId: {Bye}",
                device.DeviceId,
                device.InstanceId,
                instanceId
            );
            return Task.CompletedTask;
        }

        removalTracker.Schedule(device.DeviceId, async _ => await PerformDeviceRemoval(device));

        return Task.CompletedTask;
    }

    private async Task PerformDeviceRemoval(Device device)
    {
        try
        {
            foreach (var subscription in device.Subscriptions)
            {
                await scanner.UnsubscribeAsync(device.ScanServiceAddress, subscription.Value.Identifier);
            }

            deviceRepository.TryRemoveById(device.DeviceId, out _);
            logger.LogInformation("Device removed: {DeviceId}", device.DeviceId);
        }
        catch (Exception e)
        {
            logger.LogError(
                "Error while removing device subscription: {DeviceId}. {Message}",
                device.DeviceId,
                e.Message
            );
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

        const int maxRetries = 3;
        const int delayMs = 3000;
        for (var retryAttempt = 0; retryAttempt < maxRetries; retryAttempt++)
        {
            try
            {
                if (deviceRepository.TryGetById(deviceId, out var device))
                {
                    await UpdateDevice(device, mexAddress, instanceId, metadataVersion);
                }
                else
                {
                    await AddNewDevice(deviceId, mexAddress, type, instanceId, metadataVersion);
                }

                break;
            }
            catch (Exception ex)
            {
                if (retryAttempt < maxRetries - 1)
                {
                    logger.LogWarning(
                        "Error while adding device, retrying in {Delay}ms... (Attempt {Attempt}). {Message}",
                        delayMs,
                        retryAttempt + 1,
                        ex.Message
                    );
                    await Task.Delay(delayMs);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    private async Task UpdateDevice(Device device, string mexAddress, uint instanceId, uint metadataVersion)
    {
        if (instanceId < device.InstanceId)
            return;

        removalTracker.Cancel(device.DeviceId);

        logger.LogInformation(
            "Updating device metadata for {DeviceId}. Old Version: {OldVersion}, New Version: {NewVersion}",
            device.DeviceId,
            device.MetadataVersion,
            metadataVersion
        );

        var updatedMetadata = await scanner.GetScanDeviceMetadataAsync(device.DeviceId, mexAddress);
        var newSubscriptions = new Dictionary<SubscriptionEventType, Subscription>(device.Subscriptions);

        foreach (var subscriptionEntry in device.Subscriptions)
        {
            try
            {
                var subscription = await scanner.SubscribeAsync(
                    updatedMetadata.ScanServiceAddress,
                    subscriptionEntry.Key,
                    _scanDestinations
                );
                newSubscriptions[subscriptionEntry.Key] = subscription;
                logger.LogDebug(
                    "Resubscribed {SubscriptionId} for device {DeviceId}",
                    subscription.Identifier,
                    device.DeviceId
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "Failed to resubscribe {SubscriptionId} for device {DeviceId}. {Message}",
                    subscriptionEntry.Value.Identifier,
                    device.DeviceId,
                    ex.Message
                );
            }
        }

        var updatedDevice = device with
        {
            MexAddress = mexAddress,
            ModelName = updatedMetadata.ModelName,
            SerialNumber = updatedMetadata.SerialNumber,
            ScanServiceAddress = updatedMetadata.ScanServiceAddress,
            InstanceId = instanceId,
            MetadataVersion = metadataVersion,
            Subscriptions = newSubscriptions.ToImmutableDictionary()
        };

        deviceRepository.Update(updatedDevice);
    }

    private async Task AddNewDevice(
        string deviceId,
        string mexAddress,
        string type,
        uint instanceId,
        uint metadataVersion
    )
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
            ScanDestinations = _scanDestinations.ToImmutableList(),
            Subscriptions = subscriptions.ToImmutableDictionary(),
            ScanTickets = configuration.Value.ScanProfiles.ToDictionary(
                    e => e.Id,
                    e => new ScanTicket { Resolution = e.Resolution }
                )
                .ToImmutableDictionary(),
            InstanceId = instanceId,
            MetadataVersion = metadataVersion
        };
        deviceRepository.Add(newDevice);
        logger.LogInformation("Device added: {Device}", newDevice);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in deviceRepository.ToImmutableList())
        {
            await PerformDeviceRemoval(device);
        }
    }
}