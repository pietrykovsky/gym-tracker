using GymTracker.Data;

namespace GymTracker.Services;

public interface ITrainingActivityService
{
    Task<IEnumerable<TrainingActivity>> GetSessionActivitiesAsync(string userId, int sessionId);
    Task<TrainingActivity?> GetActivityByIdAsync(string userId, int sessionId, int activityId);
    Task<TrainingActivity> CreateActivityAsync(string userId, int sessionId, TrainingActivity activity);
    Task<TrainingActivity?> UpdateActivityAsync(string userId, int sessionId, int activityId, TrainingActivity activity);
    Task<bool> DeleteActivityAsync(string userId, int sessionId, int activityId);
    Task<bool> UpdateActivitiesOrderAsync(string userId, int sessionId, IEnumerable<(int ActivityId, int NewOrder)> newOrders);
}
