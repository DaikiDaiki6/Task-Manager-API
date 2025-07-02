using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Common;
using TaskManagerAPI.DTOs.User;

namespace TaskManagerAPI.Controllers;

/// <summary>
/// User profile management controller
/// </summary>
[Authorize]
public class UsersController : BaseController
{
    private readonly ILogger<UsersController> _logger;
    private readonly AppDbContext _dbContext;

    public UsersController(AppDbContext dbContext, ILogger<UsersController> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim);
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>User profile information</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="404">Users not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationRequest pagination)
    {
        try
        {
            var page = pagination.GetValidPage();
            var pageSize = pagination.GetValidPageSize();

            var totalCount = await _dbContext.Users.CountAsync();
            var users = await _dbContext.Users
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                })
                .ToArrayAsync();
            if (users == null || !users.Any())
            {
                return NotFound("No users found");
            }

            var response = new PaginatedResponse<object>
            {
                Data = users.Cast<object>().ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page * pageSize < totalCount,
                HasPreviousPage = page > 1
            };
            return Ok(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving users");
            return StatusCode(500, "No results");
        }
    }

    /// <summary>
    /// Gets the authenticated user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("profile")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var user = await _dbContext.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates the authenticated user's profile
    /// </summary>
    /// <param name="updateUserRequest">Updated user data</param>
    /// <returns>Updated user profile</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Invalid user data or username already exists</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("profile")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest updateUserRequest)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var existingUser = await _dbContext.Users.FindAsync(currentUserId);
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            if (existingUser.UserName != updateUserRequest.UserName)
            {
                var usernameExists = await _dbContext.Users
                    .AnyAsync(u => u.UserName == updateUserRequest.UserName && u.Id != currentUserId);
                
                if (usernameExists)
                {
                    return BadRequest("Username already exists");
                }
            }

            existingUser.UserName = updateUserRequest.UserName;
            existingUser.PassWord = updateUserRequest.PassWord;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var updatedUser = new
            {
                existingUser.Id,
                existingUser.UserName,
                existingUser.CreatedAt,
                existingUser.UpdatedAt
            };

            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "Internal server error");
        }
    }

}
