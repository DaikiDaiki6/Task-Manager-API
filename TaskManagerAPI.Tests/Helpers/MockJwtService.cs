using System;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Tests.Helpers;

public class MockJwtService : IJwtService
{
    public string GenerateToken(User user)
    {
        return $"mock-jwt-token-for-user-{user.Id}";
    }

    public int? GetUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        if (token.StartsWith("mock-jwt-token-for-user-"))
        {
            var userIdPart = token.Replace("mock-jwt-token-for-user-", "");
            if (int.TryParse(userIdPart, out int userId))
            {
                return userId;
            }
        }

        return null;
    }
}
