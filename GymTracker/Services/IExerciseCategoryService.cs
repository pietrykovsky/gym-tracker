using System;
using GymTracker.Data;

namespace GymTracker.Services;

public interface IExerciseCategoryService
{
    Task<IEnumerable<ExerciseCategory>> GetAllCategoriesAsync();
    Task<ExerciseCategory?> GetCategoryByIdAsync(int id);
    Task<ExerciseCategory?> GetCategoryWithExercisesAsync(int id);
}
