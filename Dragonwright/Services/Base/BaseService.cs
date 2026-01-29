namespace Dragonwright.Services.Base;

/// <summary>
/// The base service class provides a foundational structure for all services within the application.
/// </summary>
public abstract class BaseService : IHostedService
{
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}