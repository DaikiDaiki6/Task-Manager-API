using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerAPI.Controllers;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs.Common;
using TaskManagerAPI.DTOs.Task;
using TaskManagerAPI.Tests.Fixtures;
using TaskManagerAPI.Tests.Helpers;

namespace TaskManagerAPI.Tests.Controllers;

public class TaskControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskController _controller;
    private readonly Mock<ILogger<TaskController>> _mockLogger;

    public TaskControllerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockLogger = new Mock<ILogger<TaskController>>();
        _controller = new TaskController(_context, _mockLogger.Object);
        
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

    #region GetAllTask Tests

    [Fact]
    public async Task GetAllTask_WithValidPagination_ReturnsOkWithUserTasks()
    {
        var pagination = new PaginationRequest
        {
            Page = 1,
            PageSize = 10
        };

        var result = await _controller.GetAllTask(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeOfType<PaginatedResponse<object>>();
        
        var response = okResult.Value as PaginatedResponse<object>;
        response.Data.Should().NotBeEmpty();
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        
        foreach (var item in response.Data)
        {
            var itemType = item.GetType();
            var userId = itemType.GetProperty("UserId").GetValue(item);
            userId.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetAllTask_WithDifferentUser_ReturnsOnlyUserTasks()
    {
        SetupUserContext(2); 
        var pagination = new PaginationRequest { Page = 1, PageSize = 10 };

        var result = await _controller.GetAllTask(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as PaginatedResponse<object>;
        
        foreach (var item in response.Data)
        {
            var itemType = item.GetType();
            var userId = itemType.GetProperty("UserId").GetValue(item);
            userId.Should().Be(2);
        }
    }

    [Fact]
    public async Task GetAllTask_WithNoTasks_ReturnsEmptyPaginatedResponse()
    {
        SetupUserContext(999); 
        var pagination = new PaginationRequest { Page = 1, PageSize = 10 };

        var result = await _controller.GetAllTask(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as PaginatedResponse<object>;
        
        response.Data.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.TotalPages.Should().Be(0);
        response.HasNextPage.Should().BeFalse();
        response.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllTask_WithCustomPagination_ReturnsCorrectPage()
    {
        var pagination = new PaginationRequest
        {
            Page = 1,
            PageSize = 1 
        };

        var result = await _controller.GetAllTask(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as PaginatedResponse<object>;
        
        response.Data.Should().HaveCount(1);
        response.PageSize.Should().Be(1);
    }

    #endregion

    #region GetTaskById Tests

    [Fact]
    public async Task GetTaskById_WithValidTaskId_ReturnsOkWithTask()
    {
        var taskId = 1; 

        var result = await _controller.GetTaskById(taskId);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var task = okResult.Value;
        var taskType = task.GetType();
        taskType.GetProperty("Id").GetValue(task).Should().Be(taskId);
        taskType.GetProperty("UserId").GetValue(task).Should().Be(1);
    }

    [Fact]
    public async Task GetTaskById_WithNonExistentTask_ReturnsNotFound()
    {
        var nonExistentTaskId = 999;

        var result = await _controller.GetTaskById(nonExistentTaskId);

        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("No Task Found");
    }

    [Fact]
    public async Task GetTaskById_WithOtherUserTask_ReturnsNotFound()
    {
        var taskId = 2; 

        var result = await _controller.GetTaskById(taskId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region CreateTask Tests

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreatedWithTask()
    {
        var createRequest = new CreateTaskRequest
        {
            Title = "New Test Task",
            Description = "This is a test task description",
            Status = Models.Enums.TaskStatus.Ongoing,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var result = await _controller.CreateTask(createRequest);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        
        var task = createdResult.Value;
        var taskType = task.GetType();
        taskType.GetProperty("Title").GetValue(task).Should().Be("New Test Task");
        taskType.GetProperty("Description").GetValue(task).Should().Be("This is a test task description");
        taskType.GetProperty("Status").GetValue(task).Should().Be(Models.Enums.TaskStatus.Ongoing);
        taskType.GetProperty("UserId").GetValue(task).Should().Be(1);
        
        var savedTask = await _context.Tasks.FindAsync((int)taskType.GetProperty("Id").GetValue(task));
        savedTask.Should().NotBeNull();
        savedTask.Title.Should().Be("New Test Task");
        savedTask.UserId.Should().Be(1);
    }

    [Fact]
    public async Task CreateTask_SetsCorrectTimestamps()
    {
        var beforeCreate = DateTime.UtcNow;
        var createRequest = new CreateTaskRequest
        {
            Title = "Timestamp Test Task",
            Description = "Testing timestamps",
            Status = Models.Enums.TaskStatus.Ongoing,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var result = await _controller.CreateTask(createRequest);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        
        var task = createdResult.Value;
        var taskType = task.GetType();
        var createdAt = (DateTime)taskType.GetProperty("CreatedAt").GetValue(task);
        var updatedAt = (DateTime)taskType.GetProperty("UpdatedAt").GetValue(task);
        
        createdAt.Should().BeOnOrAfter(beforeCreate);
        updatedAt.Should().BeOnOrAfter(beforeCreate);
        createdAt.Should().BeCloseTo(updatedAt, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region EditTask Tests

    [Fact]
    public async Task EditTask_WithValidData_ReturnsOkWithUpdatedTask()
    {
        var taskId = 1; 
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Task Title",
            Description = "Updated description",
            Status = Models.Enums.TaskStatus.Finished,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        var result = await _controller.EditTask(taskId, updateRequest);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var task = okResult.Value;
        var taskType = task.GetType();
        taskType.GetProperty("Title").GetValue(task).Should().Be("Updated Task Title");
        taskType.GetProperty("Description").GetValue(task).Should().Be("Updated description");
        taskType.GetProperty("Status").GetValue(task).Should().Be(Models.Enums.TaskStatus.Finished);
        
        var updatedTask = await _context.Tasks.FindAsync(taskId);
        updatedTask.Title.Should().Be("Updated Task Title");
        updatedTask.Status.Should().Be(Models.Enums.TaskStatus.Finished);
    }

    [Fact]
    public async Task EditTask_WithNonExistentTask_ReturnsNotFound()
    {
        var nonExistentTaskId = 999;
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated description",
            Status = Models.Enums.TaskStatus.Finished,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var result = await _controller.EditTask(nonExistentTaskId, updateRequest);

        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Task not found");
    }

    [Fact]
    public async Task EditTask_UpdatesTimestamp()
    {
        var taskId = 1;
        var originalTask = await _context.Tasks.FindAsync(taskId);
        var originalUpdateTime = originalTask.UpdatedAt;
        
        await Task.Delay(1);
        
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Timestamp Update Test",
            Description = originalTask.Description,
            Status = originalTask.Status,
            DueDate = originalTask.DueDate
        };

        var result = await _controller.EditTask(taskId, updateRequest);

        result.Should().BeOfType<OkObjectResult>();
        
        var updatedTask = await _context.Tasks.FindAsync(taskId);
        updatedTask.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    #endregion

    #region DeleteTask Tests

    [Fact]
    public async Task DeleteTask_WithValidTaskId_ReturnsNoContent()
    {
        var taskId = 1; 

        var result = await _controller.DeleteTask(taskId);

        result.Should().BeOfType<NoContentResult>();
        
        var deletedTask = await _context.Tasks.FindAsync(taskId);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_WithNonExistentTask_ReturnsNotFound()
    {
        var nonExistentTaskId = 999;

        var result = await _controller.DeleteTask(nonExistentTaskId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteTask_DoesNotDeleteOtherUserTasks()
    {
        var user2TaskId = 2; 
        var initialTaskCount = await _context.Tasks.CountAsync();

        var result = await _controller.DeleteTask(user2TaskId);

        result.Should().BeOfType<NotFoundResult>();
        
        var taskCount = await _context.Tasks.CountAsync();
        taskCount.Should().Be(initialTaskCount);
        
        var task = await _context.Tasks.FindAsync(user2TaskId);
        task.Should().NotBeNull(); 
    }

    #endregion

    #region User Isolation Tests

    [Fact]
    public async Task AllEndpoints_OnlyAccessUserOwnTasks()
    {
        SetupUserContext(2);
        
        var getAllResult = await _controller.GetAllTask(new PaginationRequest { Page = 1, PageSize = 10 });
        getAllResult.Should().BeOfType<OkObjectResult>();
        var response = (getAllResult as OkObjectResult).Value as PaginatedResponse<object>;
        
        foreach (var item in response.Data)
        {
            var itemType = item.GetType();
            var userId = itemType.GetProperty("UserId").GetValue(item);
            userId.Should().Be(2, "User should only see their own tasks");
        }
        
        var getByIdResult = await _controller.GetTaskById(1); 
        getByIdResult.Should().BeOfType<NotFoundObjectResult>();
        
        var editResult = await _controller.EditTask(1, new UpdateTaskRequest
        {
            Title = "Hacked Title",
            Description = "Hacked Description",
            Status = Models.Enums.TaskStatus.Finished,
            DueDate = DateTime.UtcNow.AddDays(1)
        });
        editResult.Should().BeOfType<NotFoundObjectResult>();
        
        var deleteResult = await _controller.DeleteTask(1);
        deleteResult.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GetAllTask_WithInvalidPagination_UsesDefaultValues()
    {
        var pagination = new PaginationRequest
        {
            Page = -1, 
            PageSize = -5 
        };

        var result = await _controller.GetAllTask(pagination);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult.Value as PaginatedResponse<object>;
        
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task CreateTask_WithFutureDate_CreatesSuccessfully()
    {
        var futureDate = DateTime.UtcNow.AddMonths(6);
        var createRequest = new CreateTaskRequest
        {
            Title = "Future Task",
            Description = "Task with future due date",
            Status = Models.Enums.TaskStatus.Ongoing,
            DueDate = futureDate
        };

        var result = await _controller.CreateTask(createRequest);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        
        var task = createdResult.Value;
        var taskType = task.GetType();
        var dueDate = (DateTime)taskType.GetProperty("DueDate").GetValue(task);
        dueDate.Should().BeCloseTo(futureDate, TimeSpan.FromSeconds(1));
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}