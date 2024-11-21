using System;
using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class TrainingSession
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly Date { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual List<TrainingActivity> Activities { get; set; } = [];
}
