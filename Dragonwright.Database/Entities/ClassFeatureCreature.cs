using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class ClassFeatureCreature : IEntity<ClassFeatureCreature>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid ClassFeatureId { get; set; }
    
    [JsonIgnore]
    public ClassFeature ClassFeature { get; set; } = null!;

    /// <summary>
    /// Defines a group which this creature belongs to or is used for (e.g. "Wild Shape").
    /// </summary>
    public string CreatureGroup { get; set; } = string.Empty;
    
    /// <summary>
    /// If set, indicates an existing creature to be used for this trait, otherwise a new creature is created by
    /// supplying the creature data underneath.
    /// </summary>
    public Guid? ExistingCreatureId { get; set; }
    
    /// <summary>
    /// When set, indicates an existing creature to be used for this trait.
    /// See <see cref="ExistingCreatureId"/> for more details.
    /// </summary>
    public Creature? ExistingCreature { get; set; }
    
    /// <summary>
    /// The type of creature this trait provides. Only used if <see cref="ExistingCreatureId"/> is not set.
    /// </summary>
    public CreatureType? CreatureType { get; set; }
    
    /// <summary>
    /// The name of the creature provided by this trait. Only used if <see cref="ExistingCreatureId"/> is not set.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The maximum challenge rating of the creature provided by this trait. Only used if <see cref="ExistingCreatureId"/> is not set.
    /// </summary>
    public float? MaxChallengeRating { get; set; }
    
    /// <summary>
    /// If set, divides the character level by this value to determine the challenge rating of the creature.
    /// Only used if <see cref="ExistingCreatureId"/> is not set.
    /// </summary>
    public int? ChallengeRatingLevelDivisor { get; set; }
    
    /// <summary>
    /// A collection of movement types that are restricted for this creature. Only used if <see cref="ExistingCreatureId"/> is not set.
    /// </summary>
    public ICollection<MovementType> RestrictedMovementTypes { get; set; } = [];
    
    /// <summary>
    /// A collection of sizes that this creature can be. Only used if <see cref="ExistingCreatureId"/> is not set.
    /// </summary>
    public ICollection<Size> CreatureSizes { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<ClassFeatureCreature> builder)
    {
        builder.HasOne(rto => rto.ClassFeature)
            .WithMany(rt => rt.Creatures)
            .HasForeignKey(rto => rto.ClassFeatureId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(rto => rto.ExistingCreature)
            .WithMany()
            .HasForeignKey(rto => rto.ExistingCreatureId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(rto => rto.RestrictedMovementTypes).EnumCollection();
        builder.Property(rto => rto.CreatureSizes).EnumCollection();
    }
}