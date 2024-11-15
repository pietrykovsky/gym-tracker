using GymTracker.Data;

namespace GymTracker.Services;

public interface IUserMadeTrainingPlanService
{
    Task<IEnumerable<UserMadeTrainingPlan>> GetUserPlansAsync(string userId);
    Task<IEnumerable<UserMadeTrainingPlan>> GetUserPlansByCategoryAsync(string userId, int categoryId);
    Task<UserMadeTrainingPlan?> GetUserPlanByIdAsync(string userId, int planId);
    Task<UserMadeTrainingPlan> CreateUserPlanAsync(string userId, UserMadeTrainingPlan plan, IEnumerable<int> categoryIds);
    Task<UserMadeTrainingPlan?> UpdateUserPlanAsync(string userId, int planId, UserMadeTrainingPlan plan, IEnumerable<int> categoryIds);
    Task<bool> DeleteUserPlanAsync(string userId, int planId);
}