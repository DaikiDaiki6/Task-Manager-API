using System;

namespace TaskManagerAPI.DTOs.Auth;

/// <summary>
/// User information included in login response
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User Name
    /// </summary>
    /// <value>Default value is Empty. User Name.</value>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// User Password
    /// </summary>
    /// <value>Default value is Empty. User Password.</value>
    public string PassWord { get; set; } = string.Empty;
}
