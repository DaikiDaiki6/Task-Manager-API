using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagerAPI.Models.Entities;

[Table("Tasks")]
public class Task
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? Title { get; set; }
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string? Description { get; set; }
    [Required]
    public Enums.TaskStatus Status { get; set; }
    [Required]
    public DateTime DueDate { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime UpdatedAt { get; set; }
    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
