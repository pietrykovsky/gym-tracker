using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class UserMadeTrainingPlanService : IUserMadeTrainingPlanService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UserMadeTrainingPlanService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<UserMadeTrainingPlan>> GetUserPlansAsync(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeTrainingPlans
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserMadeTrainingPlan>> GetUserPlansByCategoryAsync(string userId, int categoryId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeTrainingPlans
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .Where(p => p.UserId == userId && p.Categories.Any(c => c.Id == categoryId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<UserMadeTrainingPlan?> GetUserPlanByIdAsync(string userId, int planId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeTrainingPlans
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == planId);
    }

    public async Task<UserMadeTrainingPlan> CreateUserPlanAsync(
        string userId,
        UserMadeTrainingPlan plan,
        IEnumerable<int> categoryIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Fetch fresh categories from database instead of using the ones in plan object
        var categories = await context.TrainingPlanCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        // Create new plan without categories first
        var newPlan = new UserMadeTrainingPlan
        {
            UserId = userId,
            Name = plan.Name,
            Description = plan.Description,
            Categories = categories  // Use freshly loaded categories
        };

        // Add activities and sets
        foreach (var activity in plan.Activities)
        {
            var newActivity = new PlanActivity
            {
                Order = activity.Order,
                ExerciseId = activity.ExerciseId,
                Sets = activity.Sets.Select(s => new ExerciseSet
                {
                    Order = s.Order,
                    Repetitions = s.Repetitions,
                    Weight = s.Weight,
                    RestAfterDuration = s.RestAfterDuration
                }).ToList()
            };
            newPlan.Activities.Add(newActivity);
        }

        await context.UserMadeTrainingPlans.AddAsync(newPlan);
        await context.SaveChangesAsync();

        return newPlan;
    }

    public async Task<UserMadeTrainingPlan?> UpdateUserPlanAsync(
        string userId,
        int planId,
        UserMadeTrainingPlan updatedPlan,
        IEnumerable<int> categoryIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var plan = await context.UserMadeTrainingPlans
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == planId);

        if (plan == null)
            return null;

        var categories = await context.TrainingPlanCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count == 0)
        {
            throw new ArgumentException("At least one valid category must be provided.");
        }

        // Update basic plan info
        plan.Name = updatedPlan.Name;
        plan.Description = updatedPlan.Description;

        // Update categories
        plan.Categories.Clear();
        plan.Categories = categories;

        // Update activities and sets
        var existingActivities = plan.Activities.ToList();
        plan.Activities.Clear();
        await context.SaveChangesAsync(); // Save to remove old activities

        foreach (var updatedActivity in updatedPlan.Activities)
        {
            updatedActivity.Order = updatedPlan.Activities.IndexOf(updatedActivity) + 1;
            foreach (var set in updatedActivity.Sets)
            {
                set.Order = updatedActivity.Sets.IndexOf(set) + 1;
            }
            plan.Activities.Add(updatedActivity);
        }

        await context.SaveChangesAsync();
        return plan;
    }

    public async Task<bool> DeleteUserPlanAsync(string userId, int planId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var plan = await context.UserMadeTrainingPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == planId);

        if (plan == null)
            return false;

        context.UserMadeTrainingPlans.Remove(plan);
        await context.SaveChangesAsync();
        return true;
    }
}