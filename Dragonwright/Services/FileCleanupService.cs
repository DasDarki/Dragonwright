using Dragonwright.Configuration;
using Dragonwright.Services.Base;
using Microsoft.Extensions.Options;

namespace Dragonwright.Services;

/// <summary>
/// Background service that periodically removes orphaned files no longer referenced by any entity.
/// </summary>
public sealed class FileCleanupService : ScheduledService
{
    private readonly ILogger _logger;
    private readonly FileStorageService _fileStorageService;

    public FileCleanupService(
        ILogger logger,
        FileStorageService fileStorageService,
        IOptions<FileStorageConfiguration> config
    ) : base(
        TimeSpan.FromMinutes(config.Value.CleanupIntervalMinutes),
        TimeSpan.FromMinutes(1))
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    /// <inheritdoc />
    protected override async Task TickAsync(CancellationToken stoppingToken)
    {
        try
        {
            var orphans = await _fileStorageService.FindOrphanedFilesAsync();

            if (orphans.Count == 0)
            {
                return;
            }

            _logger.Information("Found {Count} orphaned file(s), cleaning up...", orphans.Count);

            foreach (var orphan in orphans)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await _fileStorageService.DeleteAsync(orphan.Id);
            }

            _logger.Information("Orphan cleanup completed, removed {Count} file(s)", orphans.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during file cleanup");
        }
    }
}
