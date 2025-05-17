using System;
using System.Text.Json;
using LuxAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using LuxAPI.DAL;

public class SystemControllerTests
{
    private readonly Mock<ILogger<SystemController>> _mockLogger;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly SystemController _controller;

    public SystemControllerTests()
    {
        _mockLogger = new Mock<ILogger<SystemController>>();
        _controller = new SystemController(_mockLogger.Object, _mockDbContext.Object);
    }

    [Fact]
    public async Task GetStatus_ReturnsOkResultWithStatus()
    {
        // Act
        var result = await _controller.GetStatus() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var json = JsonSerializer.Serialize(result.Value);
        var value = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        Assert.NotNull(value);
        Assert.Equal("OK", value["Status"]);
    }

    [Fact]
    public void GetVersion_ReturnsOkResultWithVersion()
    {
        // Act
        var result = _controller.GetVersion() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var json = JsonSerializer.Serialize(result.Value);
        var value = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        Assert.NotNull(value);
        Assert.Equal("1.0.0", value["Version"]);
    }
}
