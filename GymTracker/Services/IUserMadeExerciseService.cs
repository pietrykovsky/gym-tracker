using GymTracker.Data;

namespace GymTracker.Services;

public interface IUserMadeExerciseService
{
    Task<IEnumerable<UserMadeExercise>> GetUserExercisesAsync(string userId);
    Task<UserMadeExercise?> GetUserExerciseByIdAsync(string userId, int exerciseId);
    Task<IEnumerable<UserMadeExercise>> GetUserExercisesByCategoryAsync(string userId, int categoryId);
    Task<IEnumerable<UserMadeExercise>> GetUserExercisesByDifficultyAsync(string userId, ExerciseDifficulty difficulty);
    Task<UserMadeExercise> CreateUserExerciseAsync(string userId, UserMadeExercise exercise, IEnumerable<int> categoryIds);
    Task<UserMadeExercise?> UpdateUserExerciseAsync(string userId, int exerciseId, UserMadeExercise exercise, IEnumerable<int> categoryIds);
    Task<bool> DeleteUserExerciseAsync(string userId, int exerciseId);
}