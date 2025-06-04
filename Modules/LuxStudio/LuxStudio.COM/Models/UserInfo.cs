using System.Text.Json.Serialization;

namespace LuxStudio.COM.Models;

/// <summary>
/// UserInfo represents the basic information of a user in the LuxStudio application.
/// </summary>
/// <param name="UserId">User ID as Guid</param>
/// <param name="Username">UserName as String</param>
/// <param name="Email">Email Address as String</param>
/// <param name="AvatarUrl">Avatar URL might be null if not set</param>
public record UserInfo(
    [property: JsonPropertyName("id")] Guid UserId,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("avatarFileName")] string? AvatarUrl
);
