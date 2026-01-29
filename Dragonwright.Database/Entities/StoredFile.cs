using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dragonwright.Database.Entities;

public sealed class StoredFile : IEntity<StoredFile>
{
    [Key]
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long Size { get; set; }

    public string StoragePath { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.HasIndex(f => f.StoragePath).IsUnique();
    }
}
