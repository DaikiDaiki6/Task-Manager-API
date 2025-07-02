using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerAPI.Controllers;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Common;
using TaskManagerAPI.DTOs.User;
using TaskManagerAPI.Tests.Fixtures;
using TaskManagerAPI.Tests.Helpers;

namespace TaskManagerAPI.Tests.Controllers;

public class UserControllerTest : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UsersController _controller;
    private readonly Mock<ILogger<UsersController>> _mockLogger;

    public UserControllerTest()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_context, _mockLogger.Object);

        TestDataFixture.SeedTestData(_context);

        SetupUserContext(1);
    }

    private void SetupUserContext(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"testuser{userId}")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetAllUsers_WithValidPagination_ReturnsOkWithPaginatedUsers()
    {
        var pagination = new PaginationRequest
        {
            Page = 1,
            PageSize = 10
        };

        var result = await _controller.GetAllUsers(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<PaginatedResponse<object>>();
        
        var response = okResult.Value as PaginatedResponse<object>;
        response.Data.Should().NotBeEmpty();
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllUsers_WithCustomPagination_ReturnsCorrectPage()
    {
        var pagination = new PaginationRequest
        {
            Page = 1,
            PageSize = 1 
        };

        var result = await _controller.GetAllUsers(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as PaginatedResponse<object>;
        
        response.Data.Should().HaveCount(1);
        response.HasNextPage.Should().BeTrue(); // Should have more users
        response.HasPreviousPage.Should().BeFalse(); // First page
    }

    [Fact]
    public async Task GetProfile_WithValidUser_ReturnsOkWithUserProfile()
    {
        
        var result = await _controller.GetProfile();

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var profile = okResult.Value;
        profile.Should().NotBeNull();
        
        var profileType = profile.GetType();
        profileType.GetProperty("Id").GetValue(profile).Should().Be(1);
        profileType.GetProperty("UserName").GetValue(profile).Should().Be("testuser1");
    }

    [Fact]
    public async Task GetProfile_WithNonExistentUser_ReturnsNotFound()
    {
        SetupUserContext(999);

        var result = await _controller.GetProfile();

        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ReturnsOkWithUpdatedProfile()
    {
        var updateRequest = new UpdateUserRequest
        {
            UserName = "updateduser1",
            PassWord = "UpdatedPassword123"
        };

        var result = await _controller.UpdateProfile(updateRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var updatedProfile = okResult.Value;
        var profileType = updatedProfile.GetType();
        profileType.GetProperty("UserName").GetValue(updatedProfile).Should().Be("updateduser1");
        
        var userInDb = await _context.Users.FindAsync(1);
        userInDb.UserName.Should().Be("updateduser1");
        userInDb.PassWord.Should().Be("UpdatedPassword123");
    }

    [Fact]
    public async Task UpdateProfile_WithExistingUsername_ReturnsBadRequest()
    {
        var updateRequest = new UpdateUserRequest
        {
            UserName = "testuser2", 
            PassWord = "UpdatedPassword123"
        };

        var result = await _controller.UpdateProfile(updateRequest);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Username already exists");
    }

    [Fact]
    public async Task UpdateProfile_WithSameUsername_ReturnsOkWithUpdatedProfile()
    {
        var updateRequest = new UpdateUserRequest
        {
            UserName = "testuser1", 
            PassWord = "UpdatedPassword123"
        };

        var result = await _controller.UpdateProfile(updateRequest);

        result.Should().BeOfType<OkObjectResult>();
        
        var userInDb = await _context.Users.FindAsync(1);
        userInDb.PassWord.Should().Be("UpdatedPassword123");
    }

    [Fact]
    public async Task UpdateProfile_WithNonExistentUser_ReturnsNotFound()
    {
        SetupUserContext(999);
        
        var updateRequest = new UpdateUserRequest
        {
            UserName = "newusername",
            PassWord = "NewPassword123"
        };

        var result = await _controller.UpdateProfile(updateRequest);

        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("User not found");
    }

    [Fact]
    public async Task GetAllUsers_WithInvalidPage_UsesDefaultValues()
    {
        var pagination = new PaginationRequest
        {
            Page = -1, 
            PageSize = -5 
        };

        var result = await _controller.GetAllUsers(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as PaginatedResponse<object>;
        
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllUsers_WithLargePage_ReturnsEmptyData()
    {
        var pagination = new PaginationRequest
        {
            Page = 999, 
            PageSize = 10
        };

        var result = await _controller.GetAllUsers(pagination);

        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("No users found");
    }

    [Fact]
    public async Task UpdateProfile_UpdatesTimestamp()
    {
        var originalUser = await _context.Users.FindAsync(1);
        var originalUpdateTime = originalUser.UpdatedAt;
        
        await Task.Delay(1);
        
        var updateRequest = new UpdateUserRequest
        {
            UserName = "timestamptest",
            PassWord = "UpdatedPassword123"
        };

        var result = await _controller.UpdateProfile(updateRequest);

        result.Should().BeOfType<OkObjectResult>();
        
        var updatedUser = await _context.Users.FindAsync(1);
        updatedUser.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
