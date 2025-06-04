using LuxStudio.COM.Models;
using LuxStudio.COM.Services;

namespace LuxStudio.COM.Auth;

public class AuthManager(LuxStudioConfig config)
{
    private readonly AuthService _authSvc = new(config);
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime? _generatedAt;

    /// <summary>
    /// Retrives or generates a valid access token.
    /// </summary>
    /// <returns>Return the accessToken</returns>
    /// <exception cref="InvalidOperationException">If the 10-minute login flow fails or times out, or if the authorization code is not available.</exception>
    public async Task<string> GetAccessTokenAsync()
    {
        if (
            _accessToken == null
            || _refreshToken == null
            || _generatedAt == null
            || (_generatedAt != null && _generatedAt.Value.AddSeconds(AuthService.RefreshTokenExpirationDelay) < DateTime.UtcNow))
        {
            // Need to authenticate again
            bool status = await _authSvc.StartLoginFlowAsync(600); // 10 minutes timeout
            if (!status)
            {
                throw new InvalidOperationException("Login flow failed or timed out. Please try again.");
            }
            if (string.IsNullOrEmpty(_authSvc.AuthorizationCode))
            {
                throw new InvalidOperationException("Authorization code is not available. Please ensure the login flow was completed successfully.");
            }
            (string accessToken, string refreshToken) = await _authSvc.ExchangeAuthorizationCode(_authSvc.AuthorizationCode);
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Failed to exchange authorization code for access and refresh tokens.");
            }
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _generatedAt = DateTime.UtcNow;
            return _accessToken;
        }

        if (_generatedAt != null && _generatedAt.Value.AddSeconds(AuthService.AccessTokenExpirationDelay) < DateTime.UtcNow)
        {
            // Need to refresh the access token
            (string accessToken, string refreshToken) = await _authSvc.RefreshAccessToken(_refreshToken);
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Failed to refresh access token.");
            }
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _generatedAt = DateTime.UtcNow;

            return _accessToken;
        }

        return _accessToken;
    }

    /// <summary>
    /// Returns whether the user is authenticated based on the access token, refresh token, and their generation time.
    /// </summary>
    /// <returns>True if it is, Otherwise false</returns>
    public bool IsAuthenticated()
    {
        if (_accessToken == null || _refreshToken == null || _generatedAt == null)
        {
            return false;
        }

        return _generatedAt.Value.AddSeconds(AuthService.AccessTokenExpirationDelay) > DateTime.UtcNow
            && _generatedAt.Value.AddSeconds(AuthService.RefreshTokenExpirationDelay) > DateTime.UtcNow;
    }

    /// <summary>
    /// Returns the current user information
    /// </summary>
    /// <returns
    /// Returns the user information as a UserInfo
    /// </returns>
    public async Task<UserInfo> GetUserInfoAsync() => await _authSvc.WhoAmIAsync(await GetAccessTokenAsync());
}
