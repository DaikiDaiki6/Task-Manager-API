using System;

namespace TaskManagerAPI.DTOs.User;

/// <summary>
/// Request model for updating an existing user's profile
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Updated username for the account
    /// </summary>
    /// <value>Username must be between 3 and 50 characters. Can only contain letters, numbers, and underscores. Must be unique if changed.</value>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Updated password for the account
    /// </summary>
    /// <value>Password must be between 6 and 100 characters. Must contain at least one lowercase letter, one uppercase letter, and one number.</value>
    public string PassWord { get; set; } = string.Empty;
}
