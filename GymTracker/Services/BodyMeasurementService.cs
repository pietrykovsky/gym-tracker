using Microsoft.EntityFrameworkCore;
using GymTracker.Data;

namespace GymTracker.Services;

public class BodyMeasurementService : IBodyMeasurementService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public BodyMeasurementService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<BodyMeasurement>> GetUserMeasurementsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BodyMeasurements
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<BodyMeasurement>> GetMeasurementsInRangeAsync(string userId, DateOnly startDate, DateOnly endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BodyMeasurements
            .Where(m => m.UserId == userId && m.Date >= startDate && m.Date <= endDate)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<BodyMeasurement?> GetMeasurementAsync(string userId, int measurementId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == measurementId);
    }

    public async Task<BodyMeasurement> CreateMeasurementAsync(string userId, BodyMeasurement measurement)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        measurement.UserId = userId;
        await context.BodyMeasurements.AddAsync(measurement);
        await context.SaveChangesAsync();
        return measurement;
    }

    public async Task<BodyMeasurement?> UpdateMeasurementAsync(string userId, int measurementId, BodyMeasurement updatedMeasurement)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingMeasurement = await context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == measurementId);

        if (existingMeasurement == null)
            return null;

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

        await context.SaveChangesAsync();
        return existingMeasurement;
    }

    public async Task<bool> DeleteMeasurementAsync(string userId, int measurementId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var measurement = await context.BodyMeasurements
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == measurementId);

        if (measurement == null)
            return false;

        context.BodyMeasurements.Remove(measurement);
        await context.SaveChangesAsync();
        return true;
    }
}
