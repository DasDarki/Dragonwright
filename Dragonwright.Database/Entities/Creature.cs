using Dragonwright.Database.Entities.Models;

namespace Dragonwright.Database.Entities;

public sealed class Creature : IEntity<Creature>
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = null!;
    
    public Size Size { get; set; }
    
    public CreatureType Type { get; set; }
    
    public Alignment Alignment { get; set; }
    
    public int ArmorClass { get; set; }
    
    public string ArmorClassNotes { get; set; } = string.Empty;
    
    public int HitPoints { get; set; }
    
    public int HitPointDiceCount { get; set; }
    
    public int HitPointDiceValue { get; set; }
    
    public int HitPointDiceBonus { get; set; }
    
    public int Speed { get; set; }
    
    public int FlyingSpeed { get; set; }
    
    public int SwimmingSpeed { get; set; }
    
    public int ClimbSpeed { get; set; }
    
    public float ChallengeRating { get; set; }
    
    public int XP { get; set; }
    
    public int ProficiencyBonus { get; set; }
    
    public int StrengthScore { get; set; }
    
    public int StrengthModifier => (StrengthScore - 10) / 2;
    
    public int StrengthSavingThrowBonus { get; set; }
    
    public int StrengthSavingThrow => StrengthModifier + StrengthSavingThrowBonus;
    
    public int DexterityScore { get; set; }
    
    public int DexterityModifier => (DexterityScore - 10) / 2;
    
    public int DexteritySavingThrowBonus { get; set; }
    
    public int DexteritySavingThrow => DexterityModifier + DexteritySavingThrowBonus;
    
    public int ConstitutionScore { get; set; }
    
    public int ConstitutionModifier => (ConstitutionScore - 10) / 2;
    
    public int ConstitutionSavingThrowBonus { get; set; }
    
    public int ConstitutionSavingThrow => ConstitutionModifier + ConstitutionSavingThrowBonus;
    
    public int IntelligenceScore { get; set; }
    
    public int IntelligenceModifier => (IntelligenceScore - 10) / 2;
    
    public int IntelligenceSavingThrowBonus { get; set; }
    
    public int IntelligenceSavingThrow => IntelligenceModifier + IntelligenceSavingThrowBonus;
    
    public int WisdomScore { get; set; }
    
    public int WisdomModifier => (WisdomScore - 10) / 2;
    
    public int WisdomSavingThrowBonus { get; set; }
    
    public int WisdomSavingThrow => WisdomModifier + WisdomSavingThrowBonus;
    
    public int CharismaScore { get; set; }
    
    public int CharismaModifier => (CharismaScore - 10) / 2;
    
    public int CharismaSavingThrowBonus { get; set; }
    
    public int CharismaSavingThrow => CharismaModifier + CharismaSavingThrowBonus;
    
    public IDictionary<Skill, int> SkillModifiers { get; set; } = new Dictionary<Skill, int>();
    
    public int PassivePerceptionBonus { get; set; }
    
    public int PassivePerception => 10 + WisdomModifier + (SkillModifiers.TryGetValue(Skill.Perception, out var value) ? value : 0) + PassivePerceptionBonus;
    
    public int PassiveInvestigationBonus { get; set; }
    
    public int PassiveInvestigation => 10 + IntelligenceModifier + (SkillModifiers.TryGetValue(Skill.Investigation, out var value) ? value : 0) + PassiveInvestigationBonus;
    
    public int PassiveInsightBonus { get; set; }
    
    public int PassiveInsight => 10 + WisdomModifier + (SkillModifiers.TryGetValue(Skill.Insight, out var value) ? value : 0) + PassiveInsightBonus;

    public ICollection<string> Languages { get; set; } = [];
    
    public int DarkvisionRange { get; set; }
    
    public int TruesightRange { get; set; }
    
    public int TremorsenseRange { get; set; }
    
    public int BlindsightRange { get; set; }
    
    public IDictionary<string, string> Traits { get; set; } = new Dictionary<string, string>();
    
    public ICollection<CreatureAction> Actions { get; set; } = [];
    
    public IDictionary<CreatureAction, int> LegendaryActions { get; set; } = new Dictionary<CreatureAction, int>();
    
    public ICollection<CreatureAction> LairActions { get; set; } = [];
    
    public ICollection<CreatureAction> Reactions { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<Creature> builder)
    {
        builder.Property(c => c.Size).HasConversion<string>();
        builder.Property(c => c.Type).HasConversion<string>();
        builder.Property(c => c.Alignment).HasConversion<string>();

        builder.Property(c => c.Languages).JsonCollection();
        builder.Property(c => c.SkillModifiers).JsonDictionary();
        builder.Property(c => c.Traits).JsonDictionary();
        builder.Property(c => c.Actions).JsonCollection();
        builder.Property(c => c.LegendaryActions).JsonDictionary();
        builder.Property(c => c.LairActions).JsonCollection();
        builder.Property(c => c.Reactions).JsonCollection();
    }
}