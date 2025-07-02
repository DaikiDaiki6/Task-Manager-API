using System;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerAPI.Controllers;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Auth;
using TaskManagerAPI.DTOs.User;
using TaskManagerAPI.Services;
using TaskManagerAPI.Tests.Fixtures;
using TaskManagerAPI.Tests.Helpers;

namespace TaskManagerAPI.Tests.Controllers;

public class AuthControllerTest : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthController _controller;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly IJwtService _jwtService;

    public AuthControllerTest()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _jwtService = new MockJwtService();
        _controller = new AuthController(_context, _jwtService, _mockLogger.Object);

        TestDataFixture.SeedTestData(_context);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var loginRequest = new LoginRequest
        {
            UserName = "testuser1",
            PassWord = "Password123"
        };

        var result = await _controller.Login(loginRequest);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<AuthResponse>();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequest
        {
            UserName = "testuser1",
            PassWord = "WrongPassword"
        };

        var result = await _controller.Login(loginRequest);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_WithNewUsername_ReturnsCreatedWithToken()
    {
        var registerRequest = new CreateUserRequest
        {
            UserName = "newuser",
            PassWord = "NewPassword123"
        };

        var result = await _controller.Register(registerRequest);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.Value.Should().BeOfType<AuthResponse>();
        
        var authResponse = createdResult.Value as AuthResponse;
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.User.UserName.Should().Be("newuser");
    }

    [Fact]
    public async Task Register_WithExistingUsername_ReturnsBadRequest()
    {
        var registerRequest = new CreateUserRequest
        {
            UserName = "testuser1", 
            PassWord = "Password123"
        };

        var result = await _controller.Register(registerRequest);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
