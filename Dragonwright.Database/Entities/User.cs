using System.ComponentModel.DataAnnotations;
using Dragonwright.Database.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dragonwright.Database.Entities;

public sealed class User : IEntity<User>
{
    [Key]
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public Guid? AvatarId { get; set; }

    public StoredFile? Avatar { get; set; }

    public Role Role { get; set; } = Role.User;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.Role).HasConversion<string>();

        builder.HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
