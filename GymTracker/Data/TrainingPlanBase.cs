using System;
using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public abstract class TrainingPlanBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Description must be less than 200 characters.")]
    public string? Description { get; set; } = string.Empty;
}
