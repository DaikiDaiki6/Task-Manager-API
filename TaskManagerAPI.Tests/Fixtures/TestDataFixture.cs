using System;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Models.Enums;

namespace TaskManagerAPI.Tests.Fixtures;

public class TestDataFixture
{
    public static void SeedTestData(AppDbContext context)
    {
        context.Users.RemoveRange(context.Users);
        context.Tasks.RemoveRange(context.Tasks);
        context.SaveChanges();

        var users = new List<User>
        {
            new User
            {
                Id = 1,
                UserName = "testuser1",
                PassWord = "Password123",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Id = 2,
                UserName = "testuser2", 
                PassWord = "Password123",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new User
            {
                Id = 3,
                UserName = "testuser3", 
                PassWord = "Password123",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.Users.AddRange(users);
        var tasks = new List<Models.Entities.Task>
        {
            new Models.Entities.Task
            {
                Id = 1,
                Title = "Test Task 1 - User 1",
                Description = "Test Description 1 for User 1",
                Status = Models.Enums.TaskStatus.Ongoing,
                DueDate = DateTime.UtcNow.AddDays(7),
                UserId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Models.Entities.Task
            {
                Id = 3,
                Title = "Test Task 3 - User 1",
                Description = "Another task for User 1",
                Status = Models.Enums.TaskStatus.Stopped,
                DueDate = DateTime.UtcNow.AddDays(10),
                UserId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Models.Entities.Task
            {
                Id = 2,
                Title = "Test Task 2 - User 2",
                Description = "Test Description 2 for User 2",
                Status = Models.Enums.TaskStatus.Finished,
                DueDate = DateTime.UtcNow.AddDays(14),
                UserId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}
