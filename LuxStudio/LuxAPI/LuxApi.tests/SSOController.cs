using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using LuxAPI.Controllers;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Models.DTOs;
using LuxAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SSOControllerTests
{
    private readonly Mock<ILogger<SSOController>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AppDbContext _context;
    private readonly SSOController _controller;

    public SSOControllerTests()
    {
        _mockLogger = new Mock<ILogger<SSOController>>();
        _mockConfiguration = new Mock<IConfiguration>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _mockConfiguration.SetupGet(x => x["Jwt:Key"]).Returns("YourSuperSecure32CharacterKeyHere!");
        _mockConfiguration.SetupGet(x => x["Jwt:Issuer"]).Returns("http://localhost:5269");
        _mockConfiguration.SetupGet(x => x["Jwt:Audience"]).Returns("http://localhost:5269");

        _controller = new SSOController(_mockLogger.Object, _mockConfiguration.Object, _context);
    }

    [Fact]
    public void Authorize_WithValidUser_ReturnsRedirectUrl()
    {
        var client = new Client { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), ClientSecret = "secret", RedirectUri = "http://localhost/callback" };
        _context.Clients.Add(client);
        _context.SaveChanges();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };

        var result = _controller.Authorize(client.ClientId, "code", client.RedirectUri, "state123") as OkObjectResult;

        Assert.NotNull(result);
        Assert.Contains("redirectUrl", result.Value.ToString());
    }

    [Fact]
    public void Token_WithValidAuthorizationCode_ReturnsAccessToken()
    {
        var client = new Client { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), ClientSecret = "secret", RedirectUri = "http://localhost/callback" };
        _context.Clients.Add(client);
        
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = "hashed" };
        _context.Users.Add(user);

        var authorizationCode = new AuthorizationCode
        {
            Id = Guid.NewGuid(),
            Code = "authcode123",
            ClientId = client.ClientId,
            UserId = user.Id,
            Expiry = DateTime.UtcNow.AddMinutes(10)
        };
        _context.AuthorizationCodes.Add(authorizationCode);
        _context.SaveChanges();

        var tokenRequest = new TokenRequestDto
        {
            ClientId = client.ClientId,
            ClientSecret = "secret",
            Code = "authcode123",
            GrantType = "authorization_code"
        };

        var result = _controller.Token(tokenRequest) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Contains("access_token", result.Value.ToString());
    }

    [Fact]
    public void RefreshToken_WithValidToken_ReturnsNewAccessToken()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = "hashed" };
        _context.Users.Add(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "refresh123",
            UserId = user.Id,
            Expiry = DateTime.UtcNow.AddMonths(1)
        };
        _context.RefreshTokens.Add(refreshToken);
        _context.SaveChanges();

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = "refresh123"
        };

        var result = _controller.RefreshToken(refreshRequest) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Contains("access_token", result.Value.ToString());
    }
}
