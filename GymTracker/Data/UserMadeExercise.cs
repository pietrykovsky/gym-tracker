using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class UserMadeExercise : ExerciseBase
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;

    public virtual List<ExerciseCategory> Categories { get; set; } = new List<ExerciseCategory>();

    public override MovementType GetMovementType()
    {
        if (Categories.Count > 0)
        {
            return MovementType.Compound;
        }
        return MovementType.Isolation;
    }
}
