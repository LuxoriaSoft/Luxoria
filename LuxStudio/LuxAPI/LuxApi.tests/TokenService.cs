using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using LuxAPI.Services;
using Xunit;

public class TokenServiceTests
{
    [Fact]
    public void GenerateRefreshToken_ReturnsValidBase64String()
    {
        // Act
        var refreshToken = TokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);
        
        // Ensure the token is a valid Base64 string
        try
        {
            var tokenBytes = Convert.FromBase64String(refreshToken);
            Assert.Equal(32, tokenBytes.Length); // Ensure 256-bit (32 bytes)
        }
        catch (FormatException)
        {
            Assert.False(true, "Generated token is not a valid Base64 string");
        }
    }
}
