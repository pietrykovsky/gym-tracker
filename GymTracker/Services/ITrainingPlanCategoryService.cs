using GymTracker.Data;

namespace GymTracker.Services;

public interface ITrainingPlanCategoryService
{
    Task<IEnumerable<TrainingPlanCategory>> GetAllCategoriesAsync();
    Task<TrainingPlanCategory?> GetCategoryByIdAsync(int id);
}
