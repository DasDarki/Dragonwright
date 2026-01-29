using Dragonwright.Database;
using Dragonwright.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Services;

/// <summary>
/// The database service offers functionalities related to database management and operations. It also allows
/// constructing of the related <see cref="AppDbContext"/> of EF Core and migration handling.
/// </summary>
public sealed class DatabaseService(
    ILogger logger,
    IServiceScopeFactory serviceScopeFactory
) : BaseService
{

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.Information("Applying database migrations...");
        
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(cancellationToken);
        
        logger.Information("Database migrations applied successfully.");
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="AppDbContext"/> for database operations.
    /// </summary>
    /// <returns>A scope-bound instance of <see cref="AppDbContext"/>.</returns>
    public AppDbContext CreateDbContext()
    {
        var scope = serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}