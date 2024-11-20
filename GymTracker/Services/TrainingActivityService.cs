using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class TrainingActivityService : ITrainingActivityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public TrainingActivityService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<TrainingActivity>> GetSessionActivitiesAsync(string userId, int sessionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingActivities
            .AsNoTracking()
            .Include(a => a.Exercise)
            .Include(a => a.Sets)
            .Where(a => a.TrainingSession.UserId == userId && a.TrainingSessionId == sessionId)
            .OrderBy(a => a.Order)
            .ToListAsync();
    }

    public async Task<TrainingActivity?> GetActivityByIdAsync(string userId, int sessionId, int activityId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingActivities
            .AsNoTracking()
            .Include(a => a.Exercise)
            .Include(a => a.Sets)
            .FirstOrDefaultAsync(a =>
                a.TrainingSession.UserId == userId &&
                a.TrainingSessionId == sessionId &&
                a.Id == activityId);
    }

    public async Task<TrainingActivity> CreateActivityAsync(string userId, int sessionId, TrainingActivity activity)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Verify session ownership
        var session = await context.TrainingSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);

        if (session == null)
            throw new ArgumentException("Invalid session ID or unauthorized access");

        // Get the highest current order and add 1
        var maxOrder = await context.TrainingActivities
            .Where(a => a.TrainingSessionId == sessionId)
            .MaxAsync(a => (int?)a.Order) ?? 0;

        activity.Order = maxOrder + 1;
        activity.TrainingSessionId = sessionId;

        // Ensure sets are properly ordered
        var setOrder = 1;
        foreach (var set in activity.Sets.OrderBy(s => s.Order))
        {
            set.Order = setOrder++;
        }

        await context.TrainingActivities.AddAsync(activity);
        await context.SaveChangesAsync();

        // Detach the entity to avoid tracking issues
        context.Entry(activity).State = EntityState.Detached;

        // Load related data for return
        return await context.TrainingActivities
            .AsNoTracking()
            .Include(a => a.Exercise)
            .Include(a => a.Sets)
            .FirstAsync(a => a.Id == activity.Id);
    }

    public async Task<TrainingActivity?> UpdateActivityAsync(
        string userId, int sessionId, int activityId, TrainingActivity updatedActivity)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var activity = await context.TrainingActivities
            .Include(a => a.Sets)
            .FirstOrDefaultAsync(a =>
                a.TrainingSession.UserId == userId &&
                a.TrainingSessionId == sessionId &&
                a.Id == activityId);

        if (activity == null)
            return null;

        // Preserve existing order
        var originalOrder = activity.Order;

        activity.ExerciseId = updatedActivity.ExerciseId;
        activity.Order = originalOrder; // Ensure order is preserved

        // Update sets with proper ordering
        activity.Sets.Clear();
        var setOrder = 1;
        foreach (var set in updatedActivity.Sets.OrderBy(s => s.Order))
        {
            var newSet = new ExerciseSet
            {
                Order = setOrder++,
                Repetitions = set.Repetitions,
                Weight = set.Weight,
                RestAfterDuration = set.RestAfterDuration
            };
            activity.Sets.Add(newSet);
        }

        await context.SaveChangesAsync();

        // Detach the entity to avoid tracking issues
        context.Entry(activity).State = EntityState.Detached;

        // Load fresh data for return
        return await context.TrainingActivities
            .AsNoTracking()
            .Include(a => a.Exercise)
            .Include(a => a.Sets)
            .FirstAsync(a => a.Id == activity.Id);
    }

    public async Task<bool> DeleteActivityAsync(string userId, int sessionId, int activityId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var activity = await context.TrainingActivities
            .FirstOrDefaultAsync(a =>
                a.TrainingSession.UserId == userId &&
                a.TrainingSessionId == sessionId &&
                a.Id == activityId);

        if (activity == null)
            return false;

        context.TrainingActivities.Remove(activity);
        await context.SaveChangesAsync();

        // Reorder remaining activities
        var remainingActivities = await context.TrainingActivities
            .Where(a => a.TrainingSessionId == sessionId && a.Order > activity.Order)
            .ToListAsync();

        foreach (var remainingActivity in remainingActivities)
        {
            remainingActivity.Order--;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateActivitiesOrderAsync(
        string userId,
        int sessionId,
        IEnumerable<(int ActivityId, int NewOrder)> newOrders)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var session = await context.TrainingSessions
            .Include(s => s.Activities)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);

        if (session == null)
            return false;

        foreach (var (activityId, newOrder) in newOrders)
        {
            var activity = session.Activities.FirstOrDefault(a => a.Id == activityId);
            if (activity != null)
            {
                activity.Order = newOrder;
            }
        }

        await context.SaveChangesAsync();
        return true;
    }
}