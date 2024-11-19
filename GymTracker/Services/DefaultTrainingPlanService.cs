using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class DefaultTrainingPlanService : IDefaultTrainingPlanService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public DefaultTrainingPlanService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<DefaultTrainingPlan>> GetAllPlansAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultTrainingPlans
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DefaultTrainingPlan>> GetPlansByCategoryAsync(int categoryId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultTrainingPlans
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .Where(p => p.Categories.Any(c => c.Id == categoryId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<DefaultTrainingPlan?> GetPlanByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultTrainingPlans
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Exercise)
            .Include(p => p.Activities)
                .ThenInclude(a => a.Sets)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}