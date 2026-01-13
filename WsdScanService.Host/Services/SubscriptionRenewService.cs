using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Host.Repositories;

namespace WsdScanService.Host.Services;

public class SubscriptionRenewService(
    ILogger<SubscriptionRenewService> logger,
    IOptions<ScanServiceConfiguration> configuration,
    DeviceRepository deviceRepository,
    IWsScanner scanner) : BackgroundService
{
    private readonly ConcurrentDictionary<string, DateTime> _renewalFailureTimes = new();
    
    private const int MaxMinutesBeforeRemoval = 3;

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
                        _renewalFailureTimes.TryRemove(device.DeviceId, out _);

                        logger.LogInformation(
                            "Subscription {SubscriptionId} renewed. New Expires: {Expires}",
                            subscription.Identifier,
                            newExpires
                        );
                    }
                    catch (Exception ex)
                    {
                        var firstFailure = _renewalFailureTimes.GetOrAdd(device.DeviceId, now);
                        var minutesFailed = (now - firstFailure).TotalMinutes;
                        logger.LogError(
                            ex,
                            "Failed to renew subscription {SubscriptionId} for device {DeviceId} (failure since {FirstFailureTime}, {MinutesFailed:F1} minutes)",
                            subscription.Identifier,
                            device.DeviceId,
                            firstFailure,
                            minutesFailed
                        );

                        if (minutesFailed >= MaxMinutesBeforeRemoval)
                        {
                            logger.LogWarning(
                                "Device {DeviceId} could not be resubscribed for {MaxMinutesBeforeRemoval} minutes. Removing from DeviceRepository.",
                                device.DeviceId,
                                MaxMinutesBeforeRemoval
                            );
                            
                            deviceRepository.TryRemoveById(device.DeviceId, out _);
                            _renewalFailureTimes.TryRemove(device.DeviceId, out _);
                        }

                        break;
                    }
                }
            }
        }
    }
}