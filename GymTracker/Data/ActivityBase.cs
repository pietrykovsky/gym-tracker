using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public abstract class ActivityBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    public int ExerciseId { get; set; }
    public ExerciseBase Exercise { get; set; } = null!;

    public virtual List<ExerciseSet> Sets { get; set; } = [];
}
