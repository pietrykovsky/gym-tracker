using GymTracker.Data;

namespace GymTracker.Services;

public interface IPlanActivityService
{
    Task<IEnumerable<PlanActivity>> GetPlanActivitiesAsync(int planId);
    Task<PlanActivity?> GetActivityByIdAsync(int planId, int activityId);
    Task<PlanActivity> CreateActivityAsync(PlanActivity activity);
    Task<PlanActivity?> UpdateActivityAsync(int planId, int activityId, PlanActivity activity);
    Task<bool> DeleteActivityAsync(int planId, int activityId);
    Task<bool> UpdateActivitiesOrderAsync(int planId, IEnumerable<(int ActivityId, int NewOrder)> newOrders);
}