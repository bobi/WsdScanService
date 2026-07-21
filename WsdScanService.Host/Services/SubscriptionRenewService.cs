using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;
using WsdScanService.Contracts.Scanner;
using WsdScanService.Contracts.Scanner.Entities;
using WsdScanService.Host.Repositories;

namespace WsdScanService.Host.Services;

public class SubscriptionRenewService(
    ILogger<SubscriptionRenewService> logger,
    IOptions<ScanServiceConfiguration> configuration,
    DeviceRepository deviceRepository,
    IWsScanner scanner) : BackgroundService
{
    private readonly FailureTracker _failureTracker = new();

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
        var devices = deviceRepository.ToImmutableList();
        var now = DateTime.Now;
        var threshold = now.AddSeconds(configuration.Value.RenewThreshold);

        foreach (var device in devices)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ProcessDeviceAsync(device, now, threshold);
        }
    }

    private async Task ProcessDeviceAsync(Device device, DateTime now, DateTime threshold)
    {
        var renewals = new List<(SubscriptionEventType Type, DateTime NewExpires)>();

        foreach (var (subscriptionEventType, subscription) in device.Subscriptions)
        {
            if (subscription.Expires >= threshold)
            {
                continue;
            }

            var newExpires = await TryRenewSubscriptionAsync(device, subscription, now);
            if (newExpires is null)
            {
                break;
            }

            renewals.Add((subscriptionEventType, newExpires.Value));
        }

        if (renewals.Count > 0)
        {
            ApplyRenewals(device.DeviceId, renewals);
        }
    }

    private async Task<DateTime?> TryRenewSubscriptionAsync(Device device, Subscription subscription, DateTime now)
    {
        try
        {
            logger.LogInformation(
                "Renewing subscription {SubscriptionId} for device {DeviceId}. Expires: {Expires}",
                subscription.Identifier,
                device.DeviceId,
                subscription.Expires
            );

            var newExpires = await scanner.RenewSubscriptionAsync(device.ScanServiceAddress, subscription.Identifier);

            _failureTracker.RecordSuccess(device.DeviceId);

            logger.LogInformation(
                "Subscription {SubscriptionId} renewed. New Expires: {Expires}",
                subscription.Identifier,
                newExpires
            );

            return newExpires;
        }
        catch (Exception ex)
        {
            HandleRenewalFailure(device, subscription, ex, now);
            return null;
        }
    }

    private void HandleRenewalFailure(Device device, Subscription subscription, Exception ex, DateTime now)
    {
        var (firstFailure, minutesFailed) = _failureTracker.RecordFailure(device.DeviceId, now);
        logger.LogError(
            "Failed to renew subscription {SubscriptionId} for device {DeviceId} (failure since {FirstFailureTime}, {MinutesFailed:F1} minutes), {Message}",
            subscription.Identifier,
            device.DeviceId,
            firstFailure,
            minutesFailed,
            ex.Message
        );

        if (minutesFailed < MaxMinutesBeforeRemoval)
        {
            return;
        }

        logger.LogWarning(
            "Device {DeviceId} could not be resubscribed for {MaxMinutesBeforeRemoval} minutes. Removing from DeviceRepository.",
            device.DeviceId,
            MaxMinutesBeforeRemoval
        );

        deviceRepository.TryRemoveById(device.DeviceId, out _);
        _failureTracker.RecordSuccess(device.DeviceId);
    }

    private void ApplyRenewals(string deviceId, List<(SubscriptionEventType Type, DateTime NewExpires)> renewals)
    {
        deviceRepository.UpdateAtomic(
            deviceId,
            current =>
            {
                var newSubs = current.Subscriptions;
                foreach (var (type, newExpires) in renewals)
                {
                    if (newSubs.TryGetValue(type, out var sub))
                    {
                        newSubs = newSubs.SetItem(type, sub with { Expires = newExpires });
                    }
                }

                return current with { Subscriptions = newSubs };
            }
        );
    }

    private sealed class FailureTracker
    {
        private readonly ConcurrentDictionary<string, DateTime> _failureStarts = new();

        public void RecordSuccess(string deviceId) => _failureStarts.TryRemove(deviceId, out _);

        public (DateTime FirstFailure, double MinutesFailed) RecordFailure(string deviceId, DateTime now)
        {
            var firstFailure = _failureStarts.GetOrAdd(deviceId, now);
            return (firstFailure, (now - firstFailure).TotalMinutes);
        }
    }
}
