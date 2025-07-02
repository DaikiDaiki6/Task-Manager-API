using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagerAPI.Models.Entities;
[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string PassWord { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime UpdatedAt { get; set; }
    public ICollection<Task>? Tasks { get; set; }
}

