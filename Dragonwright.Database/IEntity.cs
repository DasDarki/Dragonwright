using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dragonwright.Database;

/// <summary>
/// The base interface for all database entities.
/// </summary>
public interface IEntity<T> where T : class, IEntity<T>
{
    /// <summary>
    /// Configure the entity for Entity Framework Core.
    /// </summary>
    void Configure(EntityTypeBuilder<T> builder);
}