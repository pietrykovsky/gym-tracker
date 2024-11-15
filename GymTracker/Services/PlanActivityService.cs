using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class PlanActivityService : IPlanActivityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public PlanActivityService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<PlanActivity>> GetPlanActivitiesAsync(int planId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingActivities
            .AsNoTracking()
            .Include(a => a.Exercise)
            .Include(a => a.Sets)
            .Where(a => a.PlanId == planId)
            .OrderBy(a => a.Order)
            .ToListAsync();
    }

    public async Task<PlanActivity?> GetActivityByIdAsync(int planId, int activityId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingActivities
            .AsNoTracking()
            .Include(a => a.Exercise)
            .Include(a => a.Sets)
            .FirstOrDefaultAsync(a => a.PlanId == planId && a.Id == activityId);
    }

    public async Task<PlanActivity> CreateActivityAsync(PlanActivity activity)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Get the highest current order and add 1
        var maxOrder = await context.TrainingActivities
            .Where(a => a.PlanId == activity.PlanId)
            .MaxAsync(a => (int?)a.Order) ?? 0;

        activity.Order = maxOrder + 1;

        await context.TrainingActivities.AddAsync(activity);
        await context.SaveChangesAsync();

        return activity;
    }

    public async Task<PlanActivity?> UpdateActivityAsync(int planId, int activityId, PlanActivity updatedActivity)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var activity = await context.TrainingActivities
            .Include(a => a.Sets)
            .FirstOrDefaultAsync(a => a.PlanId == planId && a.Id == activityId);

        if (activity == null)
            return null;

        activity.ExerciseId = updatedActivity.ExerciseId;
        activity.Order = updatedActivity.Order;

        // Update sets
        activity.Sets.Clear();
        if (updatedActivity.Sets != null)
        {
            foreach (var set in updatedActivity.Sets)
            {
                activity.Sets.Add(set);
            }
        }

        await context.SaveChangesAsync();
        return activity;
    }

    public async Task<bool> DeleteActivityAsync(int planId, int activityId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var activity = await context.TrainingActivities
            .FirstOrDefaultAsync(a => a.PlanId == planId && a.Id == activityId);

        if (activity == null)
            return false;

        context.TrainingActivities.Remove(activity);
        await context.SaveChangesAsync();

        // Reorder remaining activities
        var remainingActivities = await context.TrainingActivities
            .Where(a => a.PlanId == planId && a.Order > activity.Order)
            .ToListAsync();

        foreach (var remainingActivity in remainingActivities)
        {
            remainingActivity.Order--;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateActivitiesOrderAsync(int planId, IEnumerable<(int ActivityId, int NewOrder)> newOrders)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var activities = await context.TrainingActivities
            .Where(a => a.PlanId == planId)
            .ToListAsync();

        foreach (var (activityId, newOrder) in newOrders)
        {
            var activity = activities.FirstOrDefault(a => a.Id == activityId);
            if (activity != null)
            {
                activity.Order = newOrder;
            }
        }

        await context.SaveChangesAsync();
        return true;
    }
}