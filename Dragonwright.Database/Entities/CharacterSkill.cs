namespace Dragonwright.Database.Entities;

public class CharacterSkill : IEntity<CharacterSkill>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
 
    public Skill Skill { get; set; }
    
    public int Bonus { get; set; }
    
    public Proficiency RawProficiency { get; set; }
    
    public Proficiency? OverrideProficiency { get; set; }
    
    public Proficiency Proficiency => OverrideProficiency ?? RawProficiency;
    
    public int Total => Bonus + (int)((Character?.ProficiencyBonus ?? 0) * Proficiency.ToMultiplier());
    
    public AdvantageState AdvantageState { get; set; } = AdvantageState.None;
    
    public void Configure(EntityTypeBuilder<CharacterSkill> builder)
    {
        builder.HasOne(cs => cs.Character)
            .WithMany(c => c.Skills)
            .HasForeignKey(cs => cs.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(cs => cs.Skill).HasConversion<string>();
        builder.Property(cs => cs.RawProficiency).HasConversion<string>();
        builder.Property(cs => cs.OverrideProficiency).HasConversion<string>();
        builder.Property(cs => cs.AdvantageState).HasConversion<string>();
    }
}