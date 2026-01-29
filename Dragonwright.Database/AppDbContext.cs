using Dragonwright.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dragonwright.Database;

/// <summary>
/// The Dragonswright application database context allows interaction with the underlying PostgreSQL database.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>();
        modelBuilder.Entity<RefreshToken>();
        modelBuilder.Entity<StoredFile>();
    }
}

internal static class EfCoreExtensions
{
    public static void Entity<T>(this ModelBuilder modelBuilder) where T : class, IEntity<T>, new()
    {
        var entity = new T();
        var builder = modelBuilder.Entity<T>();
        entity.Configure(builder);
    }
}