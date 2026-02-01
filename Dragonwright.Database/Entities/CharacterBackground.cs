using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dragonwright.Database.Entities;

public sealed class CharacterBackground : IEntity<CharacterBackground>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Character Character { get; set; } = null!;

    public Guid BackgroundId { get; set; }

    public Background Background { get; set; } = null!;

    /// <summary>
    /// The ability score increases chosen from the background's options (e.g., STR -> 2, DEX -> 1).
    /// </summary>
    public IDictionary<AbilityScore, int> ChosenAbilityScoreIncreases { get; set; } = new Dictionary<AbilityScore, int>();

    /// <summary>
    /// Languages chosen from the background's language options.
    /// </summary>
    public ICollection<string> ChosenLanguages { get; set; } = [];

    /// <summary>
    /// Chosen characteristics from the background's tables (personality trait, ideal, bond, flaw).
    /// </summary>
    public IDictionary<CharacteristicsType, string> ChosenCharacteristics { get; set; } = new Dictionary<CharacteristicsType, string>();

    public void Configure(EntityTypeBuilder<CharacterBackground> builder)
    {
        builder.HasOne(cb => cb.Character)
            .WithOne(c => c.Background)
            .HasForeignKey<CharacterBackground>(cb => cb.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cb => cb.Background)
            .WithMany()
            .HasForeignKey(cb => cb.BackgroundId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(cb => cb.ChosenAbilityScoreIncreases)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<IDictionary<AbilityScore, int>>(v) ?? new Dictionary<AbilityScore, int>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<AbilityScore, int>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<IDictionary<AbilityScore, int>>(JsonSerializer.Serialize(c)) ?? new Dictionary<AbilityScore, int>()
            ));

        builder.Property(cb => cb.ChosenLanguages).JsonCollection();

        builder.Property(cb => cb.ChosenCharacteristics)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<IDictionary<CharacteristicsType, string>>(v) ?? new Dictionary<CharacteristicsType, string>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<CharacteristicsType, string>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<IDictionary<CharacteristicsType, string>>(JsonSerializer.Serialize(c)) ?? new Dictionary<CharacteristicsType, string>()
            ));
    }
}
