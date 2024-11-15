using GymTracker.Data;

namespace GymTracker.Services;

public interface IExerciseSetService
{
    Task<IEnumerable<ExerciseSet>> GetActivitySetsAsync(int activityId);
    Task<ExerciseSet?> GetSetByIdAsync(int activityId, int setId);
    Task<ExerciseSet> CreateSetAsync(ExerciseSet set);
    Task<ExerciseSet?> UpdateSetAsync(int activityId, int setId, ExerciseSet set);
    Task<bool> DeleteSetAsync(int activityId, int setId);
    Task<bool> UpdateSetsOrderAsync(int activityId, IEnumerable<(int SetId, int NewOrder)> newOrders);
}