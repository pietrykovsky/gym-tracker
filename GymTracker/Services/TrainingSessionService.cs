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

    public async Task<TrainingSession> CreateCustomSessionAsync(string userId, TrainingSession session)
    {
        // Validate session
        ValidateSession(session);

        using var context = await _contextFactory.CreateDbContextAsync();

        // Create new session without activities
        var newSession = new TrainingSession
        {
            UserId = userId,
            Date = session.Date,
            Notes = session.Notes
        };

        await context.TrainingSessions.AddAsync(newSession);
        await context.SaveChangesAsync();

        // Filter and add valid activities only
        var validActivities = session.Activities
            .Where(a => a.ExerciseId > 0 && a.Sets.Count != 0)
            .OrderBy(a => a.Order)
            .ToList();

        var activityOrder = 1;
        foreach (var activity in validActivities)
        {
            var newActivity = new TrainingActivity
            {
                TrainingSessionId = newSession.Id,
                Order = activityOrder++,
                ExerciseId = activity.ExerciseId
            };

            await context.TrainingActivities.AddAsync(newActivity);
            await context.SaveChangesAsync();

            // Add sets with proper ordering
            var setOrder = 1;
            foreach (var set in activity.Sets.OrderBy(s => s.Order))
            {
                var newSet = new ExerciseSet
                {
                    ActivityId = newActivity.Id,
                    Order = setOrder++,
                    Repetitions = set.Repetitions,
                    Weight = set.Weight,
                    RestAfterDuration = set.RestAfterDuration
                };

                await context.ExerciseSets.AddAsync(newSet);
            }

            await context.SaveChangesAsync();
        }

        // Load and return the complete session
        return await context.TrainingSessions
            .Include(s => s.Activities.OrderBy(a => a.Order))
                .ThenInclude(a => a.Exercise)
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets.OrderBy(s => s.Order))
            .FirstAsync(s => s.Id == newSession.Id);
    }

    private static void ValidateSession(TrainingSession session)
    {
        // Check for valid activities
        var validActivities = session.Activities
            .Where(a => a.ExerciseId > 0 && a.Sets.Count != 0)
            .ToList();

        if (validActivities.Count == 0)
        {
            throw new ArgumentException("Training session must have at least one activity with an exercise and sets.");
        }
    }

    public async Task<TrainingSession> CreateFromPlanAsync(string userId, int planId, DateOnly date, string? notes = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // First try to find a default plan
        var defaultPlan = await context.DefaultTrainingPlans
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (defaultPlan != null)
        {
            return await CreateSessionFromPlan(userId, defaultPlan, date, notes);
        }

        // If not found, try to find a user-made plan
        var userPlan = await context.UserMadeTrainingPlans
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (userPlan != null)
        {
            return await CreateSessionFromPlan(userId, userPlan, date, notes);
        }

        throw new ArgumentException("Invalid plan ID");
    }

    private async Task<TrainingSession> CreateSessionFromPlan(
        string userId,
        TrainingPlanBase plan,
        DateOnly date,
        string? notes)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Create session first
        var session = new TrainingSession
        {
            UserId = userId,
            Date = date,
            Notes = notes
        };

        await context.TrainingSessions.AddAsync(session);
        await context.SaveChangesAsync();

        // Add activities one by one
        foreach (var planActivity in plan.Activities.OrderBy(a => a.Order))
        {
            var activity = new TrainingActivity
            {
                TrainingSessionId = session.Id,
                Order = planActivity.Order,
                ExerciseId = planActivity.ExerciseId
            };

            await context.TrainingActivities.AddAsync(activity);
            await context.SaveChangesAsync();

            // Add sets for this activity
            foreach (var planSet in planActivity.Sets.OrderBy(s => s.Order))
            {
                var set = new ExerciseSet
                {
                    ActivityId = activity.Id,
                    Order = planSet.Order,
                    Repetitions = planSet.Repetitions,
                    Weight = planSet.Weight,
                    RestAfterDuration = planSet.RestAfterDuration
                };

                await context.ExerciseSets.AddAsync(set);
            }
            await context.SaveChangesAsync();
        }

        // Load and return the complete session
        return await context.TrainingSessions
            .Include(s => s.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(s => s.Activities)
                .ThenInclude(a => a.Sets)
            .FirstAsync(s => s.Id == session.Id);
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