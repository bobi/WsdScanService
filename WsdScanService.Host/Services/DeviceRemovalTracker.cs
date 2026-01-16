using Microsoft.Extensions.Options;
using WsdScanService.Common.Configuration;

namespace WsdScanService.Host.Services;

using System.Collections.Concurrent;

public class DeviceRemovalTracker(ILogger<DeviceManager> logger, IOptions<ScanServiceConfiguration> configuration)
    : IDisposable
{
    private readonly ConcurrentDictionary<string, (CancellationTokenSource cts, Task task)> _removals = new();

    public void Schedule(string deviceId, Func<CancellationToken, Task> removalAction)
    {
        logger.LogInformation("Device marked for removal (grace period started): {DeviceId}", deviceId);

        Cancel(deviceId);

        var cts = new CancellationTokenSource();
        var task = Task.Run(
            async () =>
            {
                try
                {
                    await Task.Delay(configuration.Value.RemovalGracePeriodMs, cts.Token);

                    await removalAction(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug("Device removal cancelled for {DeviceId}", deviceId);
                }
                catch (Exception ex)
                {
                    logger.LogError("Error during device removal for {DeviceId}. {Message}", deviceId, ex.Message);
                }
            },
            cts.Token
        );

        _removals.TryAdd(deviceId, (cts, task));
    }

    public void Cancel(string deviceId)
    {
        if (_removals.TryRemove(deviceId, out var entry))
        {
            entry.cts.Cancel();
            entry.cts.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var entry in _removals.Values)
        {
            entry.cts.Cancel();
            entry.cts.Dispose();
        }

        _removals.Clear();
    }
}