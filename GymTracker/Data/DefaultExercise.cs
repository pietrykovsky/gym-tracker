namespace GymTracker.Data;

public class DefaultExercise : ExerciseBase
{
    public virtual List<ExerciseCategory> Categories { get; set; } = [];
}
