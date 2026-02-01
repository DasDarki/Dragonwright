using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dragonwright.Database.Entities;

public sealed class CharacterClass : IEntity<CharacterClass>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Character Character { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Class Class { get; set; } = null!;

    public int Level { get; set; }

    public Guid? SubclassId { get; set; }

    public Subclass? Subclass { get; set; }

    /// <summary>
    /// Whether this is the character's starting class (gets full proficiencies and starting equipment).
    /// </summary>
    public bool IsStartingClass { get; set; }

    /// <summary>
    /// Tracks current usage counts for class feature abilities. Maps ClassFeatureId/ClassFeatureActionId -> current uses.
    /// </summary>
    public IDictionary<Guid, int> ClassFeatureUsages { get; set; } = new Dictionary<Guid, int>();

    /// <summary>
    /// Skill proficiencies chosen from the class's skill options.
    /// </summary>
    public ICollection<Skill> ChosenSkillProficiencies { get; set; } = [];

    /// <summary>
    /// The options chosen for each class feature. Maps ClassFeatureId -> list of selected ClassFeatureOptionIds.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenFeatureOptions { get; set; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// Spells chosen from class feature spell lists. Maps ClassFeatureSpellId -> list of selected SpellIds.
    /// </summary>
    public IDictionary<Guid, List<Guid>> ChosenSpells { get; set; } = new Dictionary<Guid, List<Guid>>();

    /// <summary>
    /// Spell slots used for this class. Maps spell level (1-9) -> number of slots used.
    /// </summary>
    public IDictionary<int, int> SpellSlotsUsed { get; set; } = new Dictionary<int, int>();

    /// <summary>
    /// Number of pact magic slots used (for Warlock).
    /// </summary>
    public int PactSlotsUsed { get; set; }

    public void Configure(EntityTypeBuilder<CharacterClass> builder)
    {
        builder.HasOne(cc => cc.Character)
            .WithMany(c => c.Classes)
            .HasForeignKey(cc => cc.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Class)
            .WithMany()
            .HasForeignKey(cc => cc.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cc => cc.Subclass)
            .WithMany()
            .HasForeignKey(cc => cc.SubclassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(cc => cc.ClassFeatureUsages).JsonDictionary();
        builder.Property(cc => cc.ChosenSkillProficiencies).EnumCollection();
        builder.Property(cc => cc.SpellSlotsUsed).JsonDictionary();

        builder.Property(cc => cc.ChosenFeatureOptions)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<Guid, List<Guid>>>(v) ?? new Dictionary<Guid, List<Guid>>()
            )
            .Metadata.SetValueComparer(new ValueComparer<IDictionary<Guid, List<Guid>>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<IDictionary<Guid, List<Guid>>>(JsonSerializer.Serialize(c)) ?? new Dictionary<Guid, List<Guid>>()
            ));

        builder.Property(cc => cc.ChosenSpells)
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
