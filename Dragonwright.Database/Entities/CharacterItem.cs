namespace Dragonwright.Database.Entities;

public sealed class CharacterItem : IEntity<CharacterItem>
{
    [Key]
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }
    
    public Character Character { get; set; } = null!;
    
    public Guid ItemId { get; set; }
    
    public Item Item { get; set; } = null!;
    
    public int Quantity { get; set; }
    
    public string Notes { get; set; } = string.Empty;
    
    public bool Attuned { get; set; }
    
    public bool Equipped { get; set; }

    public int MaxCharges { get; set; }
    public int ChargesUsed { get; set; }

    public void Configure(EntityTypeBuilder<CharacterItem> builder)
    {
        builder.HasOne(ci => ci.Character)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ci => ci.Item)
            .WithMany()
            .HasForeignKey(ci => ci.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public int GetArmorClassBonus()
    {
        if (Character == null || Item == null || Item.RequiresAttunement && !Attuned || !Equipped)
        {
            return 0;
        }

        if (Item.RequiredAbilityScore.HasValue)
        {
            var abilityScore = Character.GetAbilityScore(Item.RequiredAbilityScore.Value);
            if (Item.RequiredAbilityScoreValue > abilityScore)
            {
                return 0;
            }
        }
        
        var armorClass = (Item.BaseArmorClass ?? 0) + Item.ArmorClassBonus;
        
        if (Item.ArmorClassBonusAbility.HasValue)
        {
            var abilityModifier = Character.GetAbilityModifier(Item.ArmorClassBonusAbility.Value);
            var maxBonus = Item.MaximumArmorClassBonusFromAbility;
            if (maxBonus > 0)
            {
                abilityModifier = Math.Min(abilityModifier, maxBonus);
            }
            
            armorClass += abilityModifier;
        }
        
        return armorClass;
    }
}