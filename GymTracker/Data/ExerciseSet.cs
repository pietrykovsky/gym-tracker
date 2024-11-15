using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class ExerciseSet
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    public int ActivityId { get; set; }
    public ActivityBase Activity { get; set; } = null!;

    [Required]
    public int Repetitions { get; set; }

    public float? Weight { get; set; }

    public int? RestAfterDuration { get; set; }
}
