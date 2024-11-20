using Microsoft.EntityFrameworkCore;
using GymTracker.Data;

namespace GymTracker.Services;

public class TrainingSessionService : ITrainingSessionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public TrainingSessionService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<TrainingSession>> GetUserSessionsAsync(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingSessions
            .AsNoTracking()
            .Include(s => s.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<TrainingSession?> GetSessionAsync(string userId, int sessionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingSessions
            .AsNoTracking()
            .Include(s => s.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);
    }

    public async Task<IEnumerable<TrainingSession>> GetSessionsInRangeAsync(
        string userId, DateOnly startDate, DateOnly endDate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingSessions
            .AsNoTracking()
            .Include(s => s.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets)
            .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<TrainingSession> CreateFromPlanAsync(
        string userId, int planId, DateOnly date, string? notes = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Get the plan with all activities and sets
        var plan = await context.DefaultTrainingPlans
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            throw new ArgumentException("Invalid plan ID");
        }

        // Create new session
        var session = new TrainingSession
        {
            UserId = userId,
            Date = date,
            Notes = notes
        };

        // Copy activities and sets from plan
        foreach (var planActivity in plan.Activities.OrderBy(a => a.Order))
        {
            var activity = new TrainingActivity
            {
                Order = planActivity.Order,
                ExerciseId = planActivity.ExerciseId
            };

            foreach (var planSet in planActivity.Sets.OrderBy(s => s.Order))
            {
                activity.Sets.Add(new ExerciseSet
                {
                    Order = planSet.Order,
                    Repetitions = planSet.Repetitions,
                    Weight = planSet.Weight,
                    RestAfterDuration = planSet.RestAfterDuration
                });
            }

            session.Activities.Add(activity);
        }

        await context.TrainingSessions.AddAsync(session);
        await context.SaveChangesAsync();

        return session;
    }

    public async Task<TrainingSession> CreateCustomSessionAsync(string userId, TrainingSession session)
    {
        if (session.Activities.Count == 0)
        {
            throw new ArgumentException("Session must have at least one activity");
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        session.UserId = userId;

        // Ensure proper ordering
        var order = 1;
        foreach (var activity in session.Activities)
        {
            activity.Order = order++;

            if (activity.Sets.Count == 0)
            {
                throw new ArgumentException($"Activity {activity.Exercise?.Name ?? "Unknown"} must have at least one set");
            }

            var setOrder = 1;
            foreach (var set in activity.Sets)
            {
                set.Order = setOrder++;
            }
        }

        await context.TrainingSessions.AddAsync(session);
        await context.SaveChangesAsync();

        return session;
    }

    public async Task<TrainingSession?> UpdateSessionAsync(
        string userId, int sessionId, TrainingSession updatedSession)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var session = await context.TrainingSessions
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);

        if (session == null)
            return null;

        session.Date = updatedSession.Date;
        session.Notes = updatedSession.Notes;

        // Remove existing activities and sets
        context.TrainingActivities.RemoveRange(session.Activities);
        await context.SaveChangesAsync();

        // Add updated activities and sets
        foreach (var activity in updatedSession.Activities.OrderBy(a => a.Order))
        {
            var newActivity = new TrainingActivity
            {
                Order = activity.Order,
                ExerciseId = activity.ExerciseId,
                TrainingSessionId = session.Id
            };

            foreach (var set in activity.Sets.OrderBy(s => s.Order))
            {
                newActivity.Sets.Add(new ExerciseSet
                {
                    Order = set.Order,
                    Repetitions = set.Repetitions,
                    Weight = set.Weight,
                    RestAfterDuration = set.RestAfterDuration
                });
            }

            session.Activities.Add(newActivity);
        }

        await context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteSessionAsync(string userId, int sessionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var session = await context.TrainingSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);

        if (session == null)
            return false;

        context.TrainingSessions.Remove(session);
        await context.SaveChangesAsync();
        return true;
    }
}