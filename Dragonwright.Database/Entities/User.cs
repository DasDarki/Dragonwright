namespace Dragonwright.Database.Entities;

public sealed class User : IEntity<User>
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public Guid? AvatarId { get; set; }

    public StoredFile? Avatar { get; set; }

    public UserRole UserRole { get; set; } = UserRole.User;
    
    public ICollection<Character> Characters { get; } = new List<Character>();

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.UserRole).HasConversion<string>();

        builder.HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
