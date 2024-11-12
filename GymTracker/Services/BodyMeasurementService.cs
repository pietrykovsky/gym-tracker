using Microsoft.EntityFrameworkCore;
using GymTracker.Data;

namespace GymTracker.Services;

public class BodyMeasurementService : IBodyMeasurementService
{
    private readonly ApplicationDbContext _context;

    public BodyMeasurementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BodyMeasurement>> GetUserMeasurementsAsync(string userId)
    {
        return await _context.BodyMeasurements
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<BodyMeasurement?> GetMeasurementAsync(string userId, int measurementId)
    {
        return await _context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == measurementId);
    }

    public async Task<BodyMeasurement?> GetLatestMeasurementAsync(string userId)
    {
        return await _context.BodyMeasurements
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<BodyMeasurement> CreateMeasurementAsync(string userId, BodyMeasurement measurement)
    {
        // Ensure the measurement is assigned to the correct user
        measurement.UserId = userId;

        await _context.BodyMeasurements.AddAsync(measurement);
        await _context.SaveChangesAsync();

        return measurement;
    }

    public async Task<BodyMeasurement?> UpdateMeasurementAsync(string userId, int measurementId, BodyMeasurement updatedMeasurement)
    {
        var existingMeasurement = await _context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == measurementId);

        if (existingMeasurement == null)
        {
            return null;
        }

        // Update only the allowed properties
        existingMeasurement.Date = updatedMeasurement.Date;
        existingMeasurement.Weight = updatedMeasurement.Weight;
        existingMeasurement.Height = updatedMeasurement.Height;
        existingMeasurement.FatMassPercentage = updatedMeasurement.FatMassPercentage;
        existingMeasurement.MuscleMassPercentage = updatedMeasurement.MuscleMassPercentage;
        existingMeasurement.WaistCircumference = updatedMeasurement.WaistCircumference;
        existingMeasurement.ChestCircumference = updatedMeasurement.ChestCircumference;
        existingMeasurement.ArmCircumference = updatedMeasurement.ArmCircumference;
        existingMeasurement.ThighCircumference = updatedMeasurement.ThighCircumference;
        existingMeasurement.Notes = updatedMeasurement.Notes;

        await _context.SaveChangesAsync();
        return existingMeasurement;
    }

    public async Task<bool> DeleteMeasurementAsync(string userId, int measurementId)
    {
        var measurement = await _context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == measurementId);

        if (measurement == null)
        {
            return false;
        }

        _context.BodyMeasurements.Remove(measurement);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BodyMeasurement>> GetMeasurementsInRangeAsync(string userId, DateOnly startDate, DateOnly endDate)
    {
        return await _context.BodyMeasurements
            .Where(m => m.UserId == userId && m.Date >= startDate && m.Date <= endDate)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }
}
