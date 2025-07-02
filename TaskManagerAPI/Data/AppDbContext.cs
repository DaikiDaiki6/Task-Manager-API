using System;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Models.Entities.Task> Tasks { get; set; }

}
