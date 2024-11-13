using GymTracker.Data;

namespace GymTracker.Services;

public interface IDefaultExerciseService
{
    Task<IEnumerable<DefaultExercise>> GetAllExercisesAsync();
    Task<IEnumerable<DefaultExercise>> GetAllExercisesByCategoryAsync(int categoryId);
    Task<DefaultExercise?> GetExerciseByIdAsync(int exerciseId);
}
