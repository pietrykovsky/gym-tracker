using GymTracker.Data;

namespace GymTracker.Services;

public interface ITrainingSessionService
{
    /// <summary>
    /// Gets all training sessions for a specific user
    /// </summary>
    Task<IEnumerable<TrainingSession>> GetUserSessionsAsync(string userId);

    /// <summary>
    /// Gets a specific session by id if it belongs to the user
    /// </summary>
    Task<TrainingSession?> GetSessionAsync(string userId, int sessionId);

    /// <summary>
    /// Gets all sessions for a user within a specific date range
    /// </summary>
    Task<IEnumerable<TrainingSession>> GetSessionsInRangeAsync(string userId, DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// Creates a new training session for a user based on an existing plan
    /// </summary>
    Task<TrainingSession> CreateFromPlanAsync(string userId, int planId, DateOnly date, string? notes = null);

    /// <summary>
    /// Creates a new custom training session for a user
    /// </summary>
    Task<TrainingSession> CreateCustomSessionAsync(string userId, TrainingSession session);

    /// <summary>
    /// Updates an existing session if it belongs to the user
    /// </summary>
    Task<TrainingSession?> UpdateSessionAsync(string userId, int sessionId, TrainingSession session);

    /// <summary>
    /// Deletes a session if it belongs to the user
    /// </summary>
    Task<bool> DeleteSessionAsync(string userId, int sessionId);
}