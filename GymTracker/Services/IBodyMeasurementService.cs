using GymTracker.Data;

namespace GymTracker.Services;

public interface IBodyMeasurementService
{
    /// <summary>
    /// Gets all measurements for a specific user
    /// </summary>
    Task<IEnumerable<BodyMeasurement>> GetUserMeasurementsAsync(ApplicationUser user);

    /// <summary>
    /// Gets a specific measurement by id if it belongs to the user
    /// </summary>
    Task<BodyMeasurement?> GetMeasurementAsync(ApplicationUser user, int measurementId);

    /// <summary>
    /// Gets the latest measurement for a user
    /// </summary>
    Task<BodyMeasurement?> GetLatestMeasurementAsync(ApplicationUser user);

    /// <summary>
    /// Creates a new measurement for a user
    /// </summary>
    Task<BodyMeasurement> CreateMeasurementAsync(ApplicationUser user, BodyMeasurement measurement);

    /// <summary>
    /// Updates an existing measurement if it belongs to the user
    /// </summary>
    Task<BodyMeasurement?> UpdateMeasurementAsync(ApplicationUser user, int measurementId, BodyMeasurement measurement);

    /// <summary>
    /// Deletes a measurement if it belongs to the user
    /// </summary>
    Task<bool> DeleteMeasurementAsync(ApplicationUser user, int measurementId);
}