namespace Dragonwright.Services.Base;

/// <summary>
/// Schedules a recurring task at specified intervals.
/// </summary>
/// <param name="interval">The time interval between each execution of the task.</param>
/// <param name="intialDelay">An optional initial delay before the first execution of the task.</param>
public abstract class ScheduledService(TimeSpan interval, TimeSpan? intialDelay = null) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(interval);
    
    /// <summary>
    /// Gets called on each timer tick.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task TickAsync(CancellationToken stoppingToken);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (intialDelay.HasValue)
        {
            await Task.Delay(intialDelay.Value, stoppingToken);
        }

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await TickAsync(stoppingToken);
        }
    }
}