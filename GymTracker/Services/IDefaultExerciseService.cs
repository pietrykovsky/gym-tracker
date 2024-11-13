using GymTracker.Data;

namespace GymTracker.Services;

public interface IDefaultExerciseService
{
    Task<IEnumerable<DefaultExercise>> GetAllExercisesAsync();
    Task<IEnumerable<DefaultExercise>> GetAllExercisesByCategoryAsync(int categoryId);
    Task<DefaultExercise?> GetExerciseByIdAsync(int exerciseId);
    Task<IEnumerable<DefaultExercise>> GetExercisesByDifficultyAsync(ExerciseDifficulty difficulty);
    Task<IEnumerable<DefaultExercise>> GetExercisesByMaxDifficultyAsync(ExerciseDifficulty maxDifficulty);
    Task<IEnumerable<DefaultExercise>> GetExercisesByDifficultyRangeAsync(ExerciseDifficulty minDifficulty, ExerciseDifficulty maxDifficulty);
}