using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class ExerciseSetService : IExerciseSetService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ExerciseSetService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<ExerciseSet>> GetActivitySetsAsync(int activityId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ExerciseSets
            .AsNoTracking()
            .Where(s => s.ActivityId == activityId)
            .OrderBy(s => s.Order)
            .ToListAsync();
    }

    public async Task<ExerciseSet?> GetSetByIdAsync(int activityId, int setId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ExerciseSets
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ActivityId == activityId && s.Id == setId);
    }

    public async Task<ExerciseSet> CreateSetAsync(ExerciseSet set)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Get the highest current order and add 1
        var maxOrder = await context.ExerciseSets
            .Where(s => s.ActivityId == set.ActivityId)
            .MaxAsync(s => (int?)s.Order) ?? 0;

        set.Order = maxOrder + 1;

        await context.ExerciseSets.AddAsync(set);
        await context.SaveChangesAsync();

        return set;
    }

    public async Task<ExerciseSet?> UpdateSetAsync(int activityId, int setId, ExerciseSet updatedSet)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var set = await context.ExerciseSets
            .FirstOrDefaultAsync(s => s.ActivityId == activityId && s.Id == setId);

        if (set == null)
            return null;

        set.Repetitions = updatedSet.Repetitions;
        set.Weight = updatedSet.Weight;
        set.RestAfterDuration = updatedSet.RestAfterDuration;
        set.Order = updatedSet.Order;

        await context.SaveChangesAsync();
        return set;
    }

    public async Task<bool> DeleteSetAsync(int activityId, int setId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var set = await context.ExerciseSets
            .FirstOrDefaultAsync(s => s.ActivityId == activityId && s.Id == setId);

        if (set == null)
            return false;

        context.ExerciseSets.Remove(set);
        await context.SaveChangesAsync();

        // Reorder remaining sets
        var remainingSets = await context.ExerciseSets
            .Where(s => s.ActivityId == activityId && s.Order > set.Order)
            .ToListAsync();

        foreach (var remainingSet in remainingSets)
        {
            remainingSet.Order--;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSetsOrderAsync(int activityId, IEnumerable<(int SetId, int NewOrder)> newOrders)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var sets = await context.ExerciseSets
            .Where(s => s.ActivityId == activityId)
            .ToListAsync();

        foreach (var (setId, newOrder) in newOrders)
        {
            var set = sets.FirstOrDefault(s => s.Id == setId);
            if (set != null)
            {
                set.Order = newOrder;
            }
        }

        await context.SaveChangesAsync();
        return true;
    }
}