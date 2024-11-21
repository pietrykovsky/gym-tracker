using BlazorBootstrap;
using GymTracker.Data;

namespace GymTracker.Services;

public interface IAnalyticsService
{
    Task<List<ExerciseAnalyticsSummary>> GetUserExercisesInRange(string userId, DateOnly startDate, DateOnly endDate);
    Task<ChartData> GetExerciseProgressionChart(string userId, int exerciseId, DateOnly startDate, DateOnly endDate);
    Task<ChartData> GetExerciseVolumeChart(string userId, int exerciseId, DateOnly startDate, DateOnly endDate);
    Task<ChartData> GetExerciseSetsRepsDistribution(string userId, int exerciseId, DateOnly startDate, DateOnly endDate);
    Task<ChartData> GetBodyMeasurementsChart(string userId, DateOnly startDate, DateOnly endDate);
    Task<ChartData> GetBodyCircumferencesChart(string userId, DateOnly startDate, DateOnly endDate);
    Task<ChartData> GetBodyCompositionChart(string userId, DateOnly startDate, DateOnly endDate);
}