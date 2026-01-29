using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dragonwright.Database.Entities;

public sealed class RefreshToken : IEntity<RefreshToken>
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string JwtId { get; set; } = null!;

    public string DeviceId { get; set; } = null!;

    public string? DeviceName { get; set; }

    public Guid TokenFamily { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime AbsoluteExpiryDate { get; set; }

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.TokenFamily);
        builder.HasIndex(rt => new { rt.UserId, rt.DeviceId });

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}
