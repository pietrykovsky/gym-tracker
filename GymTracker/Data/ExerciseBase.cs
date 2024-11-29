using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymTracker.Data;

public abstract class ExerciseBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ExerciseDifficulty Difficulty { get; set; } = ExerciseDifficulty.Beginner;

    [Required]
    public Equipment RequiredEquipment { get; set; } = Equipment.None;

    [StringLength(200, ErrorMessage = "Description must be less than 200 characters.")]
    public string? Description { get; set; } = string.Empty;

    [Required]
    public int PrimaryCategoryId { get; set; }
    public ExerciseCategory PrimaryCategory { get; set; } = null!;

    public abstract MovementType GetMovementType();
}
