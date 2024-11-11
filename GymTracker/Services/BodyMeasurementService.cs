using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class BodyMeasurementService : IBodyMeasurementService
{
    private readonly ApplicationDbContext _context;

    public BodyMeasurementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BodyMeasurement>> GetUserMeasurementsAsync(ApplicationUser user)
    {
        return await _context.BodyMeasurements
            .Where(m => m.UserId == user.Id)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<BodyMeasurement?> GetMeasurementAsync(ApplicationUser user, int measurementId)
    {
        return await _context.BodyMeasurements
            .Where(m => m.UserId == user.Id && m.Id == measurementId)
            .FirstOrDefaultAsync();
    }

    public async Task<BodyMeasurement?> GetLatestMeasurementAsync(ApplicationUser user)
    {
        return await _context.BodyMeasurements
            .Where(m => m.UserId == user.Id)
            .OrderByDescending(m => m.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<BodyMeasurement> CreateMeasurementAsync(ApplicationUser user, BodyMeasurement measurement)
    {
        measurement.UserId = user.Id;
        _context.BodyMeasurements.Add(measurement);
        await _context.SaveChangesAsync();
        return measurement;
    }

    public async Task<BodyMeasurement?> UpdateMeasurementAsync(ApplicationUser user, int measurementId, BodyMeasurement measurement)
    {
        var existingMeasurement = await _context.BodyMeasurements
            .Where(m => m.UserId == user.Id && m.Id == measurementId)
            .FirstOrDefaultAsync();

        if (existingMeasurement is null)
        {
            return null;
        }

        existingMeasurement.Date = measurement.Date;
        existingMeasurement.Weight = measurement.Weight;
        existingMeasurement.Height = measurement.Height;
        existingMeasurement.FatMassPercentage = measurement.FatMassPercentage;
        existingMeasurement.MuscleMassPercentage = measurement.MuscleMassPercentage;
        existingMeasurement.WaistCircumference = measurement.WaistCircumference;
        existingMeasurement.ChestCircumference = measurement.ChestCircumference;
        existingMeasurement.ArmCircumference = measurement.ArmCircumference;
        existingMeasurement.ThighCircumference = measurement.ThighCircumference;
        existingMeasurement.Notes = measurement.Notes;

        await _context.SaveChangesAsync();
        return existingMeasurement;
    }

    public async Task<bool> DeleteMeasurementAsync(ApplicationUser user, int measurementId)
    {
        var measurement = await _context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.Id == measurementId);
        
        if (measurement is null)
        {
            return false;
        }

        _context.BodyMeasurements.Remove(measurement);
        await _context.SaveChangesAsync();
        return true;
    }
}
