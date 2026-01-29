namespace Dragonwright.Database;

/// <summary>
/// The base interface for all database entities.
/// </summary>
public interface IEntity<T> where T : class, IEntity<T>
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }
    
    /// <summary>
    /// Configure the entity for Entity Framework Core.
    /// </summary>
    void Configure(EntityTypeBuilder<T> builder);
}