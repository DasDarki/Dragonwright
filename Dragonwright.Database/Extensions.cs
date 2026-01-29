using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dragonwright.Database;

/// <summary>
/// Extensions for EF Core entities.
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Configures a PropertyBuilder to store enum collections as strings.
    /// </summary>
    /// <param name="property">The PropertyBuilder to configure.</param>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>The configured PropertyBuilder.</returns>
    public static PropertyBuilder<ICollection<T>> EnumCollection<T>(this PropertyBuilder<ICollection<T>> property) where T : struct, Enum
    {
        property
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<T>).ToList()
            )
            .Metadata
            .SetValueComparer(GetEnumCollectionComparer<T>());
        return property;
    }
    
    /// <summary>
    /// Configures a PropertyBuilder to store collections of type T as JSON.
    /// </summary>
    /// <param name="property">The PropertyBuilder to configure.</param>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <returns>The configured PropertyBuilder.</returns>
    public static PropertyBuilder<ICollection<T>> JsonCollection<T>(this PropertyBuilder<ICollection<T>> property) where T : class
    {
        property
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<ICollection<T>>(v) ?? new List<T>()
            )
            .Metadata
            .SetValueComparer(GetCollectionComparer<T>());
        return property;
    }
    
    /// <summary>
    /// Configures a PropertyBuilder to store dictionaries with enum keys as JSON.
    /// </summary>
    /// <param name="property">The PropertyBuilder to configure.</param>
    /// <typeparam name="TKey">The enum type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <returns>The configured PropertyBuilder.</returns>
    public static PropertyBuilder<Dictionary<TKey, TValue>> JsonEnumDictionary<TKey, TValue>(this PropertyBuilder<Dictionary<TKey, TValue>> property) where TKey : struct, Enum
    {
        property
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(v) ?? new Dictionary<TKey, TValue>()
            )
            .Metadata
            .SetValueComparer(GetEnumDictionaryComparer<TKey, TValue>());
        return property;
    }
    
    /// <summary>
    /// Returns a ValueComparer for collections of type T.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <returns>A ValueComparer for ICollection of T.</returns>
    public static ValueComparer<ICollection<T>> GetCollectionComparer<T>() where T : class
    {
        return new ValueComparer<ICollection<T>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => v != null ? HashCode.Combine(a, v.GetHashCode()) : a),
            c => c.ToList()
        );
    }
    
    /// <summary>
    /// Returns a ValueComparer for collections of enum type T.
    /// </summary>
    /// <typeparam name="T">The enum type of the collection elements.</typeparam>
    /// <returns>A ValueComparer for ICollection of T.</returns>
    public static ValueComparer<ICollection<T>> GetEnumCollectionComparer<T>() where T : struct, Enum
    {
        return new ValueComparer<ICollection<T>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        );
    }
    
    /// <summary>
    /// Returns a ValueComparer for dictionaries with enum keys.
    /// </summary>
    /// <typeparam name="TKey">The enum type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <returns>A ValueComparer for IDictionary of TKey and TValue.</returns>
    public static ValueComparer<IDictionary<TKey, TValue>> GetEnumDictionaryComparer<TKey, TValue>() where TKey : struct, Enum
    {
        return new ValueComparer<IDictionary<TKey, TValue>>(
            (d1, d2) => d1 != null && d2 != null && d1.Count == d2.Count && !d1.Except(d2).Any(),
            d => d.Aggregate(0, (a, kvp) => kvp.Value == null ? a : HashCode.Combine(a, kvp.Key.GetHashCode(), kvp.Value.GetHashCode())),
            d => d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
}