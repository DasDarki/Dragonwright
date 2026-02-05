using System.Text.Json.Serialization;

namespace Dragonwright.Database.Entities;

public sealed class StartItem : IEntity<StartItem>
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid ChoiceId { get; set; }
    
    [JsonIgnore]
    public StartItemChoice Choice { get; set; } = null!;
    
    public StartItemType Type { get; set; }
    
    public Currency? Currency { get; set; }
    
    public int? CurrencyAmount { get; set; }
    
    public ICollection<WeaponType> WeaponTypes { get; set; } = [];
    
    public ICollection<WeaponProperty> WeaponProperties { get; set; } = [];
    
    public void Configure(EntityTypeBuilder<StartItem> builder)
    {
        builder.HasOne(si => si.Choice)
            .WithMany(sic => sic.Items)
            .HasForeignKey(si => si.ChoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(si => si.Type).HasConversion<string>();
        builder.Property(si => si.Currency).HasConversion<string?>();
        builder.Property(si => si.WeaponTypes).JsonCollection();
        builder.Property(si => si.WeaponProperties).JsonCollection();
    }
}