using System;
using UserEntity = TaskManagerAPI.Models.Entities.User;

namespace TaskManagerAPI.DTOs.Task;

/// <summary>
/// Request model for creating a new task
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// Title of the task
    /// </summary>
    /// <value>Task title with maximum length of 100 characters. Cannot be empty.</value>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the task
    /// </summary>
    /// <value>Task description with maximum length of 500 characters. Cannot be empty.</value>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Current status of the task
    /// </summary>
    /// <value>
    /// Task status enum:
    /// 0 = Ongoing (default),
    /// 1 = Stopped,
    /// 2 = Finished
    /// </value>
    public Models.Enums.TaskStatus Status = 0;
    /// <summary>
    /// Due date and time for task completion
    /// </summary>
    /// <value>Must be a future date. Cannot be in the past.</value>
    public DateTime DueDate { get; set; }


}
