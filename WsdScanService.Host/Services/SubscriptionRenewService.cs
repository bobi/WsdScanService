using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Repositories;
using WsdScanService.Contracts.Scanner;

namespace WsdScanService.Host.Services;

public class SubscriptionRenewService(
    ILogger<SubscriptionRenewService> logger,
    IOptions<ScanServiceConfiguration> configuration,
    IDeviceRepository deviceRepository,
    IWsScanner scanner) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRenewSubscriptionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in subscription renew loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(configuration.Value.RenewCheckInterval), stoppingToken);
        }
    }

    private async Task CheckAndRenewSubscriptionsAsync(CancellationToken stoppingToken)
    {
        var devices = deviceRepository.ToCollection();
        var now = DateTime.Now;
        var threshold = now.AddSeconds(configuration.Value.RenewThreshold);

        foreach (var device in devices)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            foreach (var subscriptionEntry in device.Subscriptions)
            {
                var subscription = subscriptionEntry.Value;

                if (subscription.Expires < threshold)
                {
                    try
                    {
                        logger.LogInformation(
                            "Renewing subscription {SubscriptionId} for device {DeviceId}. Expires: {Expires}",
                            subscription.Identifier,
                            device.DeviceId,
                            subscription.Expires
                        );

                        var newExpires = await scanner.RenewSubscriptionAsync(
                            device.ScanServiceAddress,
                            subscription.Identifier
                        );

                        subscription.Expires = newExpires;

                        logger.LogInformation(
                            "Subscription {SubscriptionId} renewed. New Expires: {Expires}",
                            subscription.Identifier,
                            newExpires
                        );
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to renew subscription {SubscriptionId} for device {DeviceId}",
                            subscription.Identifier,
                            device.DeviceId
                        );
                    }
                }
            }
        }
    }
}