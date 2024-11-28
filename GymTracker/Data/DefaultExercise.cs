namespace GymTracker.Data;

public class DefaultExercise : ExerciseBase
{
    public virtual List<ExerciseCategory> Categories { get; set; } = [];

    public override MovementType GetMovementType()
    {
        if (Categories.Count > 0)
        {
            return MovementType.Compound;
        }
        return MovementType.Isolation;
    }
}
