using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dragonwright.Database.Entities;

public sealed class CharacterFeat : IEntity<CharacterFeat>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Character Character { get; set; } = null!;

    public Guid FeatId { get; set; }

    public Feat Feat { get; set; } = null!;

    /// <summary>
    /// Where this feat was acquired from.
    /// </summary>
    public FeatSource Source { get; set; }

    /// <summary>
    /// The ID of the source that granted this feat (e.g., BackgroundId, ClassFeatureId, RaceTraitId).
    /// </summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// The ability score increase chosen for this feat (if the feat grants a choice).
    /// </summary>
    public AbilityScore? ChosenAbilityScoreIncrease { get; set; }

    /// <summary>
    /// The options chosen for this feat. Maps FeatId -> list of selected FeatOptionIds.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenOptions { get; set; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// Spells chosen from this feat's spell options. Maps FeatSpellId -> list of selected SpellIds.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenSpells { get; set; } = new Dictionary<Guid, List<Guid>>();

    public void Configure(EntityTypeBuilder<CharacterFeat> builder)
    {
        builder.HasOne(cf => cf.Character)
            .WithMany(c => c.Feats)
            .HasForeignKey(cf => cf.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cf => cf.Feat)
            .WithMany()
            .HasForeignKey(cf => cf.FeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(cf => cf.Source).HasConversion<string>();
        builder.Property(cf => cf.ChosenAbilityScoreIncrease).HasConversion<string?>();

        builder.Property(cf => cf.ChosenOptions)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(v) ?? new Dictionary<Guid, List<Guid>>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<Guid, List<Guid>>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<IDictionary<Guid, List<Guid>>>(JsonSerializer.Serialize(c)) ?? new Dictionary<Guid, List<Guid>>()
            ));

        builder.Property(cf => cf.ChosenSpells)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(v) ?? new Dictionary<Guid, List<Guid>>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<Guid, List<Guid>>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<IDictionary<Guid, List<Guid>>>(JsonSerializer.Serialize(c)) ?? new Dictionary<Guid, List<Guid>>()
            ));
    }
}
