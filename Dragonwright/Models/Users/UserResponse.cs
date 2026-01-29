using Dragonwright.Database.Entities;

namespace Dragonwright.Models.Users;

/// <summary>
/// Response model containing public user profile information.
/// </summary>
public sealed class UserResponse
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The username of the user.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The stored file ID of the user's avatar, or null if no avatar is set.
    /// </summary>
    public Guid? AvatarId { get; set; }

    /// <summary>
    /// The role of the user.
    /// </summary>
    public string Role { get; set; } = null!;

    /// <summary>
    /// Creates a <see cref="UserResponse"/> from a <see cref="User"/> entity.
    /// </summary>
    /// <param name="user">The user entity to map from.</param>
    /// <returns>A new <see cref="UserResponse"/> instance.</returns>
    public static UserResponse FromEntity(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        AvatarId = user.AvatarId,
        Role = user.Role.ToString()
    };
}
