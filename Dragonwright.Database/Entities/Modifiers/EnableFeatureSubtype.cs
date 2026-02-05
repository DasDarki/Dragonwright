namespace Dragonwright.Database.Entities.Modifiers;

public sealed class EnableFeatureSubtype : ModifierSubtype
{
    public string FeatureKey { get; set; } = string.Empty;
    public bool TwoWeaponFighting { get; set; }
    public bool UnarmoredDefense { get; set; }
    public bool JackOfAllTrades { get; set; }
    public bool RemarkableAthlete { get; set; }
    public bool SneakAttack { get; set; }
    public bool RageDamageBonus { get; set; }
    public bool MartialArtsDie { get; set; }
    public bool WildShape { get; set; }
    public bool ChannelDivinity { get; set; }
    public bool ExtraAttack { get; set; }
    public int ExtraAttackCount { get; set; } = 1;
}
