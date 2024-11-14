namespace GymTracker.Data;

public class ExerciseCategory : CategoryBase
{
    public virtual List<DefaultExercise> DefaultExercises { get; set; } = [];
    public virtual List<UserMadeExercise> UserMadeExercises { get; set; } = [];
}
