using System;
using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    int? GetUserIdFromToken(string token);
}
