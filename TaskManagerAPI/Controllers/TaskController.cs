using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Common;
using TaskManagerAPI.DTOs.Task;
using TaskEntity = TaskManagerAPI.Models.Entities.Task;

namespace TaskManagerAPI.Controllers;

/// <summary>
/// Task management controller for authenticated users
/// </summary>
[Authorize]
public class TaskController : BaseController
{
    private readonly ILogger<TaskController> _logger;
    private readonly AppDbContext _dbContext;

    public TaskController(AppDbContext dbContext, ILogger<TaskController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(userIdClaim);
    }

    /// <summary>
    /// Gets all tasks for the authenticated user with pagination
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <returns>Paginated list of user's tasks</returns>
    /// <response code="200">Tasks retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTask([FromQuery] PaginationRequest pagination)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var page = pagination.GetValidPage();
            var pageSize = pagination.GetValidPageSize();

            var totalCount = await _dbContext.Tasks
                .Where(t => t.UserId == currentUserId)
                .CountAsync();
            var tasks = await _dbContext.Tasks
                .Include(t => t.User)
                .Where(t => t.UserId == currentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.DueDate,
                    t.CreatedAt,
                    t.UpdatedAt,
                    t.UserId,
                    User = t.User != null ? new
                    {
                        t.User.Id,
                        t.User.UserName,
                    } : null
                })
                .ToListAsync();

            var response = new PaginatedResponse<object>
            {
                Data = tasks.Cast<object>().ToList(),
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
            _logger.LogError(e, "Error retrieving tasks");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets task that correspond to the param task id
    /// </summary>
    /// <param name="id">Task Id</param>
    /// <returns>Get task information</returns>
    /// <response code="200">Task get successfully</response>
    /// <response code="401">Invalid task data</response>
    /// <response code="404">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTaskById(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var existingTask = await _dbContext.Tasks
                .Include(t => t.User)
                .Where(t => t.Id == id && t.UserId == currentUserId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.DueDate,
                    t.UserId,
                    User = t.User != null ? new
                    {
                        t.User.Id,
                        t.User.UserName,
                    } : null
                })
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingTask == null)
            {
                return NotFound("No Task Found");
            }
            return Ok(existingTask);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving task");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new task for the authenticated user
    /// </summary>
    /// <param name="createTaskRequest">Task creation data</param>
    /// <returns>Created task information</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid task data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest createTaskRequest)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var newTask = new TaskEntity
            {
                Title = createTaskRequest.Title,
                Description = createTaskRequest.Description,
                Status = createTaskRequest.Status,
                DueDate = createTaskRequest.DueDate,
                UserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Tasks.Add(newTask);
            await _dbContext.SaveChangesAsync();

            var taskWithUser = await _dbContext.Tasks
                .Include(t => t.User)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.DueDate,
                    t.CreatedAt,
                    t.UpdatedAt,
                    t.UserId,
                    User = t.User != null ? new
                    {
                        t.User.Id,
                        t.User.UserName,
                    } : null
                })
                .FirstOrDefaultAsync(t => t.Id == newTask.Id);
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, taskWithUser);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating task");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Edit a task for the corresponing Task Id
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateTaskRequest">Task creation data</param>
    /// <returns>Updated task information</returns>
    /// <response code="201">Task updated successfully</response>
    /// <response code="400">Invalid task data</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EditTask(int id, [FromBody] UpdateTaskRequest updateTaskRequest)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var existingTask = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);

            if (existingTask == null)
            {
                return NotFound("Task not found");
            }

            existingTask.Title = updateTaskRequest.Title;
            existingTask.Description = updateTaskRequest.Description;
            existingTask.Status = updateTaskRequest.Status;
            existingTask.DueDate = updateTaskRequest.DueDate;
            existingTask.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var updatedTask = await _dbContext.Tasks
                .Include(t => t.User)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.DueDate,
                    t.CreatedAt,
                    t.UpdatedAt,
                    t.UserId,
                    User = t.User != null ? new
                    {
                        t.User.Id,
                        t.User.UserName,
                    } : null
                })
                .FirstOrDefaultAsync(t => t.Id == id);

            return Ok(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a task for the corresponing Task Id
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Deleted task information</returns>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="400">Invalid task data</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var existingTask = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUserId);
            if (existingTask == null)
            {
                return NotFound();
            }

            _dbContext.Tasks.Remove(existingTask);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            return StatusCode(500, "Internal server error");
        }
    }
}
