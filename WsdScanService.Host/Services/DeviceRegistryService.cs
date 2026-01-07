using WsdScanService.Common;
using WsdScanService.Contracts.Entities;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Contracts.ScanService;
using WsdScanService.Discovery.Messages;
using WsdScanService.Discovery.Services;
using WsdScanService.Scanner.Services;

namespace WsdScanService.Host.Services;

public class DeviceRegistryService(
    ILogger<DeviceRegistryService> logger,
    DiscoveryPubSub<IMessage> pubSub,
    IDeviceRepository deviceRepository,
    WsTransferClientService wsMetadataClient,
    WsEventingClientService wsEventingClient,
    WsWsdScanClientService wsWsdScanClient) : IHostedService
{
    private Guid _subscriberId;

    private async Task HandleMessage(IMessage message, CancellationToken _)
    {
        switch (message)
        {
            case AddDevice device:
            {
                await AddDevice(device);

                break;
            }
            case RemoveDevice device:
            {
                await RemoveDevice(device);
                break;
            }
        }
    }

    private async Task RemoveDevice(RemoveDevice device)
    {
        var existingDevice = deviceRepository.GetById(device.DeviceId);
        var subscription = existingDevice?.Subscriptions[Xd.ScanService.CallbackActions.ScanAvailableEvent];
        var scanServiceAddress = existingDevice?.ScanDeviceMetadata?.ScanServiceAddress;

        if (existingDevice != null && subscription != null && scanServiceAddress != null)
        {
            await wsEventingClient.UnsubscribeAsync(
                scanServiceAddress,
                subscription.Identifier
            );
        }

        deviceRepository.RemoveById(device.DeviceId);

        logger.LogInformation("Device removed: {DeviceId}", device.DeviceId);
    }

    private async Task AddDevice(AddDevice device)
    {
        logger.LogInformation("New device discovered: {DeviceId} at {Address}", device.DeviceId,
            device.Address);

        var newDevice = new Device
        {
            Address = device.Address,
            DeviceId = device.DeviceId,
            Type = device.Type,
            MexAddress = device.Address,
        };

        var scanDeviceMetadata = await wsMetadataClient.GetScanDeviceMetadataAsync(newDevice);

        logger.LogInformation("Scan Device Metadata: {@ScanDeviceMetadata}", scanDeviceMetadata);

        newDevice.ScanDeviceMetadata = scanDeviceMetadata;

        var scanServiceAddress = newDevice.ScanDeviceMetadata.ScanServiceAddress;

        newDevice.ScannerConfiguration = await wsWsdScanClient.GetScannerElementsAsync<ScannerConfigurationType>(
            scanServiceAddress,
            Xd.ScanService.GetScannerElementsRequestedElements.ScannerConfiguration
        );

        newDevice.DefaultScanTicket = await wsWsdScanClient.GetScannerElementsAsync<ScanTicketType>(
            scanServiceAddress,
            Xd.ScanService.GetScannerElementsRequestedElements.DefaultScanTicket);

        newDevice.Subscriptions[Xd.ScanService.CallbackActions.ScanAvailableEvent] =
            await wsEventingClient.SubscribeAsync(
                scanServiceAddress,
                Xd.ScanService.CallbackActions.ScanAvailableEvent
            );


        deviceRepository.Add(newDevice);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriberId = pubSub.Subscribe(HandleMessage);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        pubSub.Unsubscribe(_subscriberId);

        foreach (var device in deviceRepository.ToCollection())
        {
            await RemoveDevice(new RemoveDevice(device.DeviceId));
        }
    }
}