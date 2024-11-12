using GymTracker.Data;

namespace GymTracker.Services;

public interface IBodyMeasurementService
{
    /// <summary>
    /// Gets all measurements for a specific user
    /// </summary>
    Task<IEnumerable<BodyMeasurement>> GetUserMeasurementsAsync(string userId);

    /// <summary>
    /// Gets a specific measurement by id if it belongs to the user
    /// </summary>
    Task<BodyMeasurement?> GetMeasurementAsync(string userId, int measurementId);

    /// <summary>
    /// Gets the latest measurement for a user
    /// </summary>
    Task<BodyMeasurement?> GetLatestMeasurementAsync(string userId);

    /// <summary>
    /// Creates a new measurement for a user
    /// </summary>
    Task<BodyMeasurement> CreateMeasurementAsync(string userId, BodyMeasurement measurement);

    /// <summary>
    /// Updates an existing measurement if it belongs to the user
    /// </summary>
    Task<BodyMeasurement?> UpdateMeasurementAsync(string userId, int measurementId, BodyMeasurement measurement);

    /// <summary>
    /// Deletes a measurement if it belongs to the user
    /// </summary>
    Task<bool> DeleteMeasurementAsync(string userId, int measurementId);
}