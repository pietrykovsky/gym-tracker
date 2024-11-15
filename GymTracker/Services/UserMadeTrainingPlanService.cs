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
            .Where(p => p.UserId == userId && p.Categories.Any(c => c.Id == categoryId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<UserMadeTrainingPlan?> GetUserPlanByIdAsync(string userId, int planId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeTrainingPlans
            .AsNoTracking()
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == planId);
    }

    public async Task<UserMadeTrainingPlan> CreateUserPlanAsync(
        string userId,
        UserMadeTrainingPlan plan,
        IEnumerable<int> categoryIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var categories = await context.TrainingPlanCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count == 0)
        {
            throw new ArgumentException("At least one valid category must be provided.");
        }

        plan.UserId = userId;
        plan.Categories = categories;

        await context.UserMadeTrainingPlans.AddAsync(plan);
        await context.SaveChangesAsync();

        return plan;
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

        plan.Name = updatedPlan.Name;
        plan.Description = updatedPlan.Description;
        plan.Categories.Clear();
        plan.Categories = categories;

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