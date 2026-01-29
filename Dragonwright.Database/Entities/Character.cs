using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dragonwright.Database.Entities;

public sealed class Character : IEntity<Character>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public Guid? AvatarId { get; set; }
    
    public StoredFile? Avatar { get; set; }

    #region Configuration Properties

    public ICollection<SourceType> Sources { get; set; } = [];
    
    public AdvancementType AdvancementType { get; set; } = AdvancementType.Milestone;
    
    public HitPointType HitPointType { get; set; } = HitPointType.Fixed;
    
    public AbilityScoreGenerationMethod AbilityScoreGenerationMethod { get; set; } = AbilityScoreGenerationMethod.PointBuy;
    
    public bool OptionalClassFeatures { get; set; }
    
    public bool CustomizeOrigin { get; set; }
    
    public bool ExceedLevelCap { get; set; }

    #endregion

    #region Character Stats
    
    public int Level => Classes.Sum(c => c.Level);
    
    public int ProficiencyBonus => 2 + (Level - 1) / 4;
    
    public CharacterRace? Race { get; set; } 
    
    public CharacterBackground? Background { get; set; }
    
    public ICollection<CharacterClass> Classes { get; set; } = [];
    
    public ICollection<CharacterAbility> Abilities { get; set; } = [];
    
    public ICollection<CharacterSkill> Skills { get; set; } = [];
    
    public ICollection<CharacterFeat> Feats { get; set; } = [];
    
    public ICollection<CharacterSpell> Spells { get; set; } = [];

    public int MovementSpeed { get; set; }
    
    public int SwimmingSpeed { get; set; }
    
    public int FlyingSpeed { get; set; }
    
    public int InspirationPoints { get; set; }
    
    public int TemporaryHitPoints { get; set; }
    
    public int CurrentHitPoints { get; set; }
    
    public int MaximumHitPoints { get; set; }
    
    public int InitiativeBonus { get; set; }
    
    public int Initiative => Abilities.FirstOrDefault(a => a.Ability == AbilityScore.Dexterity)?.Modifier + InitiativeBonus ?? 0;
    
    public int BaseArmorClass { get; set; }
    
    public int ArmorClassBonus { get; set; }
    
    public int ArmorClass => BaseArmorClass + ArmorClassBonus + GetArmorClassBonusByEquipment();
    
    public int PassivePerceptionBonus { get; set; }

    public int PassivePerception => 10 + GetPassiveBonusFromSkill(Skill.Perception) + PassivePerceptionBonus;
    
    public int PassiveInvestigationBonus { get; set; }
    
    public int PassiveInvestigation => 10 + GetPassiveBonusFromSkill(Skill.Investigation) + PassiveInvestigationBonus;
    
    public int PassiveInsightBonus { get; set; }
    
    public int PassiveInsight => 10 + GetPassiveBonusFromSkill(Skill.Insight) + PassiveInsightBonus;

    #endregion

    #region Character State
    
    public int XP { get; set; }
    
    public int ExhaustionLevel { get; set; }
    
    public ICollection<Condition> Conditions { get; set; } = [];
    
    public IDictionary<DamageType, List<DefenseState>> DamageDefenses { get; set; } = new Dictionary<DamageType, List<DefenseState>>();
    
    public IDictionary<Condition, List<DefenseState>> ConditionDefenses { get; set; } = new Dictionary<Condition, List<DefenseState>>();

    #endregion

    #region Additions
    
    public ICollection<string> SavingThrowAdvantages { get; set; } = [];
    
    public ICollection<string> SavingThrowDisadvantages { get; set; } = [];

    public int BlindsightRange { get; set; }
    
    [MaxLength(500)]
    public string BlindsightNote { get; set; } = string.Empty;
    
    public int DarkvisionRange { get; set; }
    
    [MaxLength(500)]
    public string DarkvisionNote { get; set; } = string.Empty;
    
    public int TremorsenseRange { get; set; }
    
    [MaxLength(500)]
    public string TremorsenseNote { get; set; } = string.Empty;
    
    public int TruesightRange { get; set; }
    
    [MaxLength(500)]
    public string TruesightNote { get; set; } = string.Empty;

    #endregion

    #region Proficiencies & Training
    
    public ICollection<string> Languages { get; set; } = [];

    #endregion

    #region Inventory
    
    public bool CountMoneyWeight { get; set; }
    
    public int Gold { get; set; }
    
    public int Electrum { get; set; }
    
    public int Silver { get; set; }
    
    public int Copper { get; set; }
    
    public int ArrowQuiver { get; set; }
    
    public int BoltQuiver { get; set; }
    
    public ICollection<CharacterItem> Items { get; set; } = [];

    #endregion

    #region Details
    
    public Lifestyle Lifestyle { get; set; } = Lifestyle.Modest;

    public Alignment Alignment { get; set; } = Alignment.TrueNeutral;
    
    public Gender Gender { get; set; } = Gender.Unspecified;
    
    public Size Size { get; set; } = Size.Medium;
    
    public int Age { get; set; }
    
    public int HeightInInches { get; set; }
    
    public int HeightInFeet => HeightInInches / 12;
    
    public int HeightInRemainingInches => HeightInInches % 12;
    
    public int HeightInCentimeters => (int)(HeightInInches * 2.54);
    
    public int WeightInPounds { get; set; }
    
    public int WeightInKilograms => (int)(WeightInPounds * 0.453592);
    
    [MaxLength(100)]
    public string Skin { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Hair { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Eyes { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Appearance { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Faith { get; set; } = string.Empty;
    
    public ICollection<string> PersonalityTraits { get; set; } = [];
    
    public ICollection<string> Ideals { get; set; } = [];
    
    public ICollection<string> Bonds { get; set; } = [];
    
    public ICollection<string> Flaws { get; set; } = [];
    
    public ICollection<string> Organizations { get; set; } = [];
    
    public ICollection<string> Allies { get; set; } = [];
    
    public ICollection<string> Enemies { get; set; } = [];

    public string Backstory { get; set; } = string.Empty;
    
    public string Notes { get; set; } = string.Empty;

    #endregion
    
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.HasOne(h => h.User)
            .WithMany(u => u.Characters)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(h => h.Avatar)
            .WithMany()
            .HasForeignKey(h => h.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(c => c.AdvancementType).HasConversion<string>();
        builder.Property(c => c.HitPointType).HasConversion<string>();
        builder.Property(c => c.AbilityScoreGenerationMethod).HasConversion<string>();
        
        builder.Property(c => c.Lifestyle).HasConversion<string>();
        builder.Property(c => c.Alignment).HasConversion<string>();
        
        builder.Property(c => c.Sources).EnumCollection();
        builder.Property(c => c.Conditions).EnumCollection();
        builder.Property(c => c.DamageDefenses)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<DamageType, List<DefenseState>>>(v) ?? new Dictionary<DamageType, List<DefenseState>>()
            )
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<DamageType, List<DefenseState>>>(
                (c1, c2) => JsonSerializer.Serialize(c1) == JsonSerializer.Serialize(c2),
                c => c == null ? 0 : JsonSerializer.Serialize(c).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<DamageType, List<DefenseState>>>(JsonSerializer.Serialize(c)) ?? new Dictionary<DamageType, List<DefenseState>>()
            ));
        
        builder.Property(c => c.SavingThrowAdvantages).JsonCollection();
        builder.Property(c => c.SavingThrowDisadvantages).JsonCollection();
        builder.Property(c => c.Languages).JsonCollection();
        builder.Property(c => c.Organizations).JsonCollection();
        builder.Property(c => c.Allies).JsonCollection();
        builder.Property(c => c.Enemies).JsonCollection();
        
        builder.Property(c => c.PersonalityTraits).JsonCollection();
        builder.Property(c => c.Ideals).JsonCollection();
        builder.Property(c => c.Bonds).JsonCollection();
        builder.Property(c => c.Flaws).JsonCollection();
    }
    
    public int GetAbilityScore(AbilityScore ability)
    {
        var targetAbility = Abilities.FirstOrDefault(a => a.Ability == ability);
        if (targetAbility == null)
        {
            return 0;
        }

        return targetAbility.Score;
    }
    
    public int GetAbilityModifier(AbilityScore ability)
    {
        var targetAbility = Abilities.FirstOrDefault(a => a.Ability == ability);
        if (targetAbility == null)
        {
            return 0;
        }

        return targetAbility.Modifier;
    }
    
    private int GetPassiveBonusFromSkill(Skill skill)
    {
        var targetSkill = Skills.FirstOrDefault(s => s.Skill == skill);
        if (targetSkill == null)
        {
            return 0;
        }

        var bonus = targetSkill.Total;
        switch (targetSkill.AdvantageState)
        {
            case AdvantageState.Advantage:
                bonus = +5;
                break;
            case AdvantageState.Disadvantage:
                bonus = -5;
                break;
            case AdvantageState.None:
            default:
                break;
        }

        return bonus;
    }

    private int GetArmorClassBonusByEquipment()
    {
        if (Items.Count == 0)
        {
            return 0;
        }
        
        var highestArmorBonus = 0;
        var highestShieldBonus = 0;
        foreach (var item in Items)
        {
            if (item.Item.Type.IsArmor())
            {
                var armorBonus = item.GetArmorClassBonus();
                if (armorBonus > highestArmorBonus)
                {
                    highestArmorBonus = armorBonus;
                }
            }
            else if (item.Item.Type == ItemType.Shield)
            {
                var shieldBonus = item.GetArmorClassBonus();
                if (shieldBonus > highestShieldBonus)
                {
                    highestShieldBonus = shieldBonus;
                }
            }
        }
        
        return highestArmorBonus + highestShieldBonus;
    }
}