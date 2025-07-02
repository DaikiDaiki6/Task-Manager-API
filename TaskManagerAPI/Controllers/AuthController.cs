using System;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Auth;
using TaskManagerAPI.DTOs.User;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Services;
using LoginRequestEntity = TaskManagerAPI.DTOs.Auth.LoginRequest;

namespace TaskManagerAPI.Controllers;

/// <summary>
/// Authentication controller for user login and registration
/// </summary>
public class AuthController : BaseController
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext dbContext, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="loginRequest">User credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestEntity loginRequest)
    {
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == loginRequest.UserName && u.PassWord == loginRequest.PassWord);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = _jwtService.GenerateToken(user);
            var expires = DateTime.UtcNow.AddMinutes(60);

            var response = new AuthResponse
            {
                Token = token,
                Expires = expires,
                User = new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    CreatedAt = user.CreatedAt
                }
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Registers a new user and returns a JWT token
    /// </summary>
    /// <param name="registerRequest">User registration data</param>
    /// <returns>JWT token and user information</returns>
    /// <response code="201">Registration successful</response>
    /// <response code="400">Username already exists or validation failed</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest registerRequest)
    {
        try
        {
            var existingUser = await _dbContext.Users
                .AnyAsync(u => u.UserName == registerRequest.UserName);

            if (existingUser)
            {
                return BadRequest("Username already exists");
            }

            var newUser = new User
            {
                UserName = registerRequest.UserName,
                PassWord = registerRequest.PassWord,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            var token = _jwtService.GenerateToken(newUser);
            var expires = DateTime.UtcNow.AddMinutes(60);

            var response = new AuthResponse
            {
                Token = token,
                Expires = expires,
                User = new UserInfo
                {
                    Id = newUser.Id,
                    UserName = newUser.UserName,
                    CreatedAt = newUser.CreatedAt
                }
            };

            return CreatedAtAction(nameof(Login), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, "Internal server error");
        }
    }
}
