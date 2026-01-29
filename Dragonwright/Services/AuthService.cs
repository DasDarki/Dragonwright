using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dragonwright.Configuration;
using Dragonwright.Database;
using Dragonwright.Database.Entities;
using Dragonwright.Models.Auth;
using Dragonwright.Services.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dragonwright.Services;

/// <summary>
/// Service responsible for authentication operations including user registration, login, token generation, and refresh token rotation.
/// </summary>
public sealed class AuthService(
    ILogger logger,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AuthConfiguration> authConfiguration,
    TokenValidationParameters tokenValidationParameters
) : BaseService
{
    private readonly AuthConfiguration _authConfiguration = authConfiguration.Value;

    /// <summary>
    /// Registers a new user and returns authentication tokens.
    /// </summary>
    /// <param name="request">The registration request containing username, password, and device ID.</param>
    /// <returns>Authentication tokens if successful, null if username already exists.</returns>
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        await using var dbContext = CreateDbContext();

        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUser != null)
        {
            logger.Warning("Registration failed: Username {Username} already exists", request.Username);
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        logger.Information("User {Username} registered successfully", request.Username);

        return await GenerateTokensAsync(dbContext, user, request.DeviceId, request.DeviceName, null);
    }

    /// <summary>
    /// Authenticates a user and returns authentication tokens.
    /// </summary>
    /// <param name="request">The login request containing username, password, and device ID.</param>
    /// <returns>Authentication tokens if successful, null if credentials are invalid.</returns>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        await using var dbContext = CreateDbContext();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            logger.Warning("Login failed: Invalid credentials for username {Username}", request.Username);
            return null;
        }

        await RevokeDeviceTokensAsync(dbContext, user.Id, request.DeviceId);

        logger.Information("User {Username} logged in from device {DeviceId}", request.Username, request.DeviceId);

        return await GenerateTokensAsync(dbContext, user, request.DeviceId, request.DeviceName, null);
    }

    /// <summary>
    /// Refreshes authentication tokens using a valid refresh token. Implements token rotation and reuse detection.
    /// </summary>
    /// <param name="request">The refresh request containing the access token and refresh token.</param>
    /// <returns>New authentication tokens if successful, null if the refresh token is invalid or a security breach is detected.</returns>
    public async Task<AuthResponse?> RefreshAsync(RefreshRequest request)
    {
        await using var dbContext = CreateDbContext();

        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            logger.Warning("Refresh failed: Invalid access token");
            return null;
        }

        var jwtId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(jwtId) || string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            logger.Warning("Refresh failed: Missing or invalid claims in access token");
            return null;
        }

        var storedToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            logger.Warning("Refresh failed: Refresh token not found");
            return null;
        }

        if (storedToken.UserId != userId)
        {
            logger.Warning("Refresh failed: Token does not belong to user {UserId}", userId);
            return null;
        }

        if (storedToken.JwtId != jwtId)
        {
            logger.Warning("Refresh failed: JWT ID mismatch for user {UserId}", userId);
            return null;
        }

        if (storedToken.IsRevoked)
        {
            logger.Warning("Security breach detected: Revoked refresh token used for user {UserId}, device {DeviceId}. Revoking all tokens in family.",
                userId, storedToken.DeviceId);
            await RevokeTokenFamilyAsync(dbContext, storedToken.TokenFamily);
            return null;
        }

        if (storedToken.IsUsed)
        {
            logger.Warning("Security breach detected: Already used refresh token for user {UserId}, device {DeviceId}. Revoking all tokens in family.",
                userId, storedToken.DeviceId);
            await RevokeTokenFamilyAsync(dbContext, storedToken.TokenFamily);
            return null;
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            logger.Warning("Refresh failed: Refresh token expired for user {UserId}", userId);
            storedToken.IsRevoked = true;
            await dbContext.SaveChangesAsync();
            return null;
        }

        if (storedToken.AbsoluteExpiryDate < DateTime.UtcNow)
        {
            logger.Warning("Refresh failed: Token family absolute expiry reached for user {UserId}. User must re-authenticate.", userId);
            await RevokeTokenFamilyAsync(dbContext, storedToken.TokenFamily);
            return null;
        }

        storedToken.IsUsed = true;
        await dbContext.SaveChangesAsync();

        logger.Information("Tokens refreshed for user {Username}, device {DeviceId}",
            storedToken.User.Username, storedToken.DeviceId);

        return await GenerateTokensAsync(
            dbContext,
            storedToken.User,
            storedToken.DeviceId,
            storedToken.DeviceName,
            (storedToken.TokenFamily, storedToken.AbsoluteExpiryDate));
    }

    /// <summary>
    /// Revokes a specific refresh token (logout from device).
    /// </summary>
    /// <param name="userId">The ID of the user requesting logout.</param>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <returns>True if the token was revoked, false if not found or unauthorized.</returns>
    public async Task<bool> LogoutAsync(Guid userId, string refreshToken)
    {
        await using var dbContext = CreateDbContext();

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

        if (storedToken == null)
        {
            return false;
        }

        await RevokeTokenFamilyAsync(dbContext, storedToken.TokenFamily);

        logger.Information("User {UserId} logged out from device {DeviceId}", userId, storedToken.DeviceId);

        return true;
    }

    /// <summary>
    /// Revokes all refresh tokens for a user (logout from all devices).
    /// </summary>
    /// <param name="userId">The ID of the user requesting logout.</param>
    /// <returns>The number of sessions terminated.</returns>
    public async Task<int> LogoutAllAsync(Guid userId)
    {
        await using var dbContext = CreateDbContext();

        var activeTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        await dbContext.SaveChangesAsync();

        logger.Information("User {UserId} logged out from all {Count} devices", userId, activeTokens.Count);

        return activeTokens.Count;
    }

    private async Task<AuthResponse> GenerateTokensAsync(
        AppDbContext dbContext,
        User user,
        string deviceId,
        string? deviceName,
        (Guid Family, DateTime AbsoluteExpiry)? existingFamily)
    {
        var jwtId = Guid.NewGuid().ToString();
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_authConfiguration.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfiguration.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessTokenExpiry,
            Issuer = _authConfiguration.Issuer,
            Audience = _authConfiguration.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(_authConfiguration.RefreshTokenExpirationMinutes);

        var tokenFamily = existingFamily?.Family ?? Guid.NewGuid();
        var absoluteExpiry = existingFamily?.AbsoluteExpiry
            ?? DateTime.UtcNow.AddDays(_authConfiguration.RefreshTokenAbsoluteExpirationDays);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            JwtId = jwtId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            TokenFamily = tokenFamily,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = refreshTokenExpiry,
            AbsoluteExpiryDate = absoluteExpiry,
            IsUsed = false,
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiry
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = tokenValidationParameters.ValidateIssuer,
            ValidateAudience = tokenValidationParameters.ValidateAudience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = tokenValidationParameters.ValidateIssuerSigningKey,
            ValidIssuer = tokenValidationParameters.ValidIssuer,
            ValidAudience = tokenValidationParameters.ValidAudience,
            IssuerSigningKey = tokenValidationParameters.IssuerSigningKey
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static async Task RevokeTokenFamilyAsync(AppDbContext dbContext, Guid tokenFamily)
    {
        var familyTokens = await dbContext.RefreshTokens
            .Where(rt => rt.TokenFamily == tokenFamily)
            .ToListAsync();

        foreach (var token in familyTokens)
        {
            token.IsRevoked = true;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task RevokeDeviceTokensAsync(AppDbContext dbContext, Guid userId, string deviceId)
    {
        var deviceTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in deviceTokens)
        {
            token.IsRevoked = true;
        }

        await dbContext.SaveChangesAsync();
    }

    private AppDbContext CreateDbContext()
    {
        var scope = serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
