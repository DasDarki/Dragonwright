namespace Dragonwright.Database.Entities;

public sealed class CharacterAbility : IEntity<CharacterAbility>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
    
    public AbilityScore Ability { get; set; }
    
    public int RawScore { get; set; }
    
    public int ScoreBonus { get; set; }
    
    public int Score => RawScore + ScoreBonus;
    
    public int Modifier => (int) Math.Floor((Score - 10) / 2.0);
    
    public Proficiency SavingThrowProficiency => OverrideSavingThrowProficiency ?? RawSavingThrowProficiency;
    
    public Proficiency RawSavingThrowProficiency { get; set; }
    
    public Proficiency? OverrideSavingThrowProficiency { get; set; }
    
    public int SavingThrowBonus { get; set; }

    public int SavingThrow => Modifier + SavingThrowBonus + (int)((Character?.ProficiencyBonus ?? 0) * SavingThrowProficiency.ToMultiplier());
    
    public void Configure(EntityTypeBuilder<CharacterAbility> builder)
    {
        builder.HasOne(ca => ca.Character)
            .WithMany(c => c.Abilities)
            .HasForeignKey(ca => ca.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(ca => ca.Ability).HasConversion<string>();
        builder.Property(ca => ca.RawSavingThrowProficiency).HasConversion<string>();
        builder.Property(ca => ca.OverrideSavingThrowProficiency).HasConversion<string>();
    }
}