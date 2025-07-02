using System;
using System.ComponentModel.DataAnnotations;
using UserEntity = TaskManagerAPI.Models.Entities.User;

namespace TaskManagerAPI.DTOs.Task;

/// <summary>
/// Request model for updating an existing task
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Updated title of the task
    /// </summary>
    /// <value>Task title with maximum length of 100 characters. Cannot be empty.</value>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated detailed description of the task
    /// </summary>
    /// <value>Task description with maximum length of 500 characters. Cannot be empty.</value>
    public string? Description { get; set; }
    /// <summary>
    /// Updated status of the task
    /// </summary>
    /// <value>
    /// Task status enum:
    /// 0 = Ongoing,
    /// 1 = Stopped,
    /// 2 = Finished
    /// </value>
    public Models.Enums.TaskStatus Status { get; set; }
    /// <summary>
    /// Updated due date and time for task completion
    /// </summary>
    /// <value>Must be a future date. Cannot be in the past.</value>
    public DateTime DueDate { get; set; }
}
