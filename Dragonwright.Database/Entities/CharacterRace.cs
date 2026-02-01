using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dragonwright.Database.Entities;

public sealed class CharacterRace : IEntity<CharacterRace>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Character Character { get; set; } = null!;

    public Guid RaceId { get; set; }

    public Race Race { get; set; } = null!;

    /// <summary>
    /// Tracks current usage counts for race trait abilities. Maps RaceTraitId/RaceTraitActionId -> current uses.
    /// </summary>
    public IDictionary<Guid, int> RaceTraitUsages { get; set; } = new Dictionary<Guid, int>();

    /// <summary>
    /// The options chosen for each race trait. Maps RaceTraitId -> list of selected RaceTraitOptionIds.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenTraitOptions { get; set; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// Spells chosen from race trait spell lists. Maps RaceTraitSpellId -> list of selected SpellIds.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenSpells { get; set; } = new Dictionary<Guid, List<Guid>>();

    public void Configure(EntityTypeBuilder<CharacterRace> builder)
    {
        builder.HasOne(cr => cr.Character)
            .WithOne(c => c.Race)
            .HasForeignKey<CharacterRace>(cr => cr.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.Race)
            .WithMany()
            .HasForeignKey(cr => cr.RaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(cr => cr.RaceTraitUsages).JsonDictionary();

        builder.Property(cr => cr.ChosenTraitOptions)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(v) ?? new Dictionary<Guid, List<Guid>>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<Guid, List<Guid>>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<IDictionary<Guid, List<Guid>>>(JsonSerializer.Serialize(c)) ?? new Dictionary<Guid, List<Guid>>()
            ));

        builder.Property(cr => cr.ChosenSpells)
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
