using System;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models.Entities;
using TaskEntity = TaskManagerAPI.Models.Entities.Task;

namespace TaskManagerAPI.Data;

public class SeedData
{
    public static async System.Threading.Tasks.Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any() || context.Tasks.Any())
        {
            return;
        }

        var users = new List<User>
        {
            new User
            {
                UserName = "john_doe",
                PassWord = "Password123",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                UserName = "jane_smith",
                PassWord = "SecurePass456",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                UserName = "mike_wilson",
                PassWord = "MyPassword789",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                UserName = "sarah_connor",
                PassWord = "Terminator123",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                UserName = "admin_user",
                PassWord = "AdminPass999",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var tasks = new List<TaskEntity>
        {
            // John's tasks
            new TaskEntity
            {
                Title = "Complete project proposal",
                Description = "Write a comprehensive project proposal for the new client",
                Status = Models.Enums.TaskStatus.Stopped,
                DueDate = DateTime.UtcNow.AddDays(5),
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskEntity
            {
                Title = "Review code changes",
                Description = "Review and approve pending pull requests",
                Status = Models.Enums.TaskStatus.Finished,
                DueDate = DateTime.UtcNow.AddDays(2),
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new TaskEntity
            {
                Title = "Team meeting preparation",
                Description = "Prepare agenda and materials for weekly team meeting",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(-1),
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },

            // Jane's tasks
            new TaskEntity
            {
                Title = "Database optimization",
                Description = "Optimize database queries and improve performance",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(7),
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskEntity
            {
                Title = "API documentation",
                Description = "Update API documentation with new endpoints",
                Status = Models.Enums.TaskStatus.Finished,
                DueDate = DateTime.UtcNow.AddDays(10),
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskEntity
            {
                Title = "Security audit",
                Description = "Conduct security audit on authentication system",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(14),
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },

            // Mike's tasks
            new TaskEntity
            {
                Title = "Bug fixes",
                Description = "Fix reported bugs in the payment module",
                Status = Models.Enums.TaskStatus.Stopped,
                DueDate = DateTime.UtcNow.AddDays(3),
                UserId = users[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow
            },
            new TaskEntity
            {
                Title = "Unit tests",
                Description = "Write unit tests for new features",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(8),
                UserId = users[2].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },

            // Sarah's tasks
            new TaskEntity
            {
                Title = "UI design review",
                Description = "Review and approve new UI designs",
                Status = Models.Enums.TaskStatus.Finished,
                DueDate = DateTime.UtcNow.AddDays(-2),
                UserId = users[3].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskEntity
            {
                Title = "User training materials",
                Description = "Create training materials for new users",
                Status = Models.Enums.TaskStatus.Stopped,
                DueDate = DateTime.UtcNow.AddDays(12),
                UserId = users[3].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },

            // Admin tasks
            new TaskEntity
            {
                Title = "Server maintenance",
                Description = "Perform routine server maintenance and updates",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(1),
                UserId = users[4].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskEntity
            {
                Title = "Backup verification",
                Description = "Verify that all backups are working correctly",
                Status = Models.Enums.TaskStatus.Finished,
                DueDate = DateTime.UtcNow.AddDays(-3),
                UserId = users[4].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },

            // Additional tasks for better pagination testing
            new TaskEntity
            {
                Title = "Code refactoring",
                Description = "Refactor legacy code to improve maintainability",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(15),
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddHours(-12),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new TaskEntity
            {
                Title = "Performance testing",
                Description = "Run performance tests on the new features",
                Status = Models.Enums.TaskStatus.Stopped,
                DueDate = DateTime.UtcNow.AddDays(6),
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new TaskEntity
            {
                Title = "Client presentation",
                Description = "Prepare and deliver presentation to client",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(9),
                UserId = users[2].Id,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();
    }
}
