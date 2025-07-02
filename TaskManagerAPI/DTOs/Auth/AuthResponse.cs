using System;

namespace TaskManagerAPI.DTOs.Auth;

/// <summary>
/// Authentication response containing JWT token and user information
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT authentication token
    /// </summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>
    /// Token expiration date and time
    /// </summary>
    public DateTime Expires { get; set; }
    /// <summary>
    /// Authenticated user information
    /// </summary>
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// User information included in authentication response
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Account creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
