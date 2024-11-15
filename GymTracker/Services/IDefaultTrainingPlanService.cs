using GymTracker.Data;

namespace GymTracker.Services;

public interface IDefaultTrainingPlanService
{
    Task<IEnumerable<DefaultTrainingPlan>> GetAllPlansAsync();
    Task<IEnumerable<DefaultTrainingPlan>> GetPlansByCategoryAsync(int categoryId);
    Task<DefaultTrainingPlan?> GetPlanByIdAsync(int id);
}