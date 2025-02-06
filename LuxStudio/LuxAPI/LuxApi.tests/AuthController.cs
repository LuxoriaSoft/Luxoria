using LuxAPI.Controllers;
using LuxAPI.DAL;
using LuxAPI.Models;
using LuxAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

public class AuthControllerTests
{
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthController _controller;
    private readonly AppDbContext _context;

    public AuthControllerTests()
    {
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockConfiguration = new Mock<IConfiguration>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _mockConfiguration.SetupGet(x => x["Jwt:Key"]).Returns("YourSuperSecure32CharacterKeyHere!");
        _mockConfiguration.SetupGet(x => x["Jwt:Issuer"]).Returns("testissuer");
        _mockConfiguration.SetupGet(x => x["Jwt:Audience"]).Returns("testaudience");

        _controller = new AuthController(_mockLogger.Object, _context, _mockConfiguration.Object);
    }

    [Fact]
    public void Register_WithValidUser_ReturnsOk()
    {
        var userDto = new UserRegistrationModelDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var result = _controller.Register(userDto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Register_WithExistingUsername_ReturnsBadRequest()
    {
        _context.Users.Add(new User { Username = "existinguser", Email = "test1@example.com", PasswordHash = "hashed" });
        _context.SaveChanges();

        var userDto = new UserRegistrationModelDto
        {
            Username = "existinguser",
            Email = "test2@example.com",
            Password = "Password123!"
        };

        var result = _controller.Register(userDto) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void Login_WithValidUser_ReturnsToken()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");
        _context.Users.Add(new User { Username = "testuser", Email = "test@example.com", PasswordHash = hashedPassword });
        _context.SaveChanges();

        var loginDto = new UserLoginModelDto
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var result = _controller.Login(loginDto) as OkObjectResult;

        Assert.NotNull(result);
        Debug.WriteLine(result);
        Assert.Contains("token", result.Value.ToString());
    }

    [Fact]
    public void Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!", BCrypt.Net.BCrypt.GenerateSalt(12));

        _context.Users.Add(new User { Username = "testuser", Email = "test@example.com", PasswordHash = hashedPassword });
        _context.SaveChanges();

        var loginDto = new UserLoginModelDto
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        var result = _controller.Login(loginDto) as UnauthorizedObjectResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public void WhoAmI_WithValidClaims_ReturnsUserInfo()
    {
        var userId = Guid.NewGuid().ToString();
        var username = "testuser";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var result = _controller.WhoAmI() as OkObjectResult;
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result.Value);
        var userInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        Assert.NotNull(userInfo);

        Assert.Equal(userId, userInfo["userId"]);
        Assert.Equal(username, userInfo["username"]);



    }
}
