using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WsdScanService.Discovery.Services;

using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

public class DiscoveryPubSub<T>(ILogger<DiscoveryPubSub<T>> logger, int bufferSize = 1024) : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, Func<T, CancellationToken, Task>> _subscribers = new();

    private readonly Channel<T> _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(bufferSize)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    });

    protected override async Task ExecuteAsync(CancellationToken ctsToken)
    {
        if (ctsToken.IsCancellationRequested) return;

        await foreach (var item in _channel.Reader.ReadAllAsync(ctsToken))
        {
            try
            {
                if (ctsToken.IsCancellationRequested) break;

                foreach (var handler in _subscribers.Values)
                {
                    if (ctsToken.IsCancellationRequested) break;

                    try
                    {
                        await handler(item, ctsToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                }

                if (ctsToken.IsCancellationRequested) break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in scan job processor loop");

                await Task.Delay(100, ctsToken);
            }
        }
    }

    public Guid Subscribe(Func<T, CancellationToken, Task> handler)
    {
        var id = Guid.NewGuid();

        _subscribers[id] = handler;

        return id;
    }

    public void Unsubscribe(Guid subscriberId)
    {
        _subscribers.TryRemove(subscriberId, out _);
    }

    public async Task PublishAsync(T item, CancellationToken ctsToken = default)
    {
        await _channel.Writer.WriteAsync(item, ctsToken);
    }

    public override Task StopAsync(CancellationToken ctsToken)
    {
        _subscribers.Clear();
        _channel.Writer.TryComplete();

        return base.StopAsync(ctsToken);
    }
}