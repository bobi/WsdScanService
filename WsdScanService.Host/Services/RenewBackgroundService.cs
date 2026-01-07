namespace WsdScanService.Host.Services;

public class RenewBackgroundService(ILogger<RenewBackgroundService> logger) : BackgroundService
{
    private static int SecondsUntilMidnight()
    {
        return (int)(DateTime.Today.AddDays(1.0) - DateTime.Now).TotalSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var countdown = SecondsUntilMidnight();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (--countdown <= 0)
                {
                    await OnTimerFiredAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in scan job processor loop");
            }
            finally
            {
                countdown = SecondsUntilMidnight();
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task OnTimerFiredAsync(CancellationToken stoppingToken)
    {
        // await Task.Delay(2000, stoppingToken);
    }
}