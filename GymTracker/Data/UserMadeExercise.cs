using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class UserMadeExercise : ExerciseBase
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;

    public virtual List<ExerciseCategory> Categories { get; set; } = [];
}
