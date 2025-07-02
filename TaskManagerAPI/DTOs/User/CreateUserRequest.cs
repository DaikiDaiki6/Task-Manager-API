using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.DTOs.User;

/// <summary>
/// Request model for creating a new user account
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Username for the new account
    /// </summary>
    /// <value>Username must be between 3 and 50 characters. Can only contain letters, numbers, and underscores. Must be unique.</value>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for the new account
    /// </summary>
    /// <value>Password must be between 6 and 100 characters. Must contain at least one lowercase letter, one uppercase letter, and one number.</value>
    public string PassWord { get; set; } = string.Empty;
}
