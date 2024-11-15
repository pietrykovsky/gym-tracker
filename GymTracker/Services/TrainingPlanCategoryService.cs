using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class TrainingPlanCategoryService : ITrainingPlanCategoryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public TrainingPlanCategoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<TrainingPlanCategory>> GetAllCategoriesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingPlanCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<TrainingPlanCategory?> GetCategoryByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TrainingPlanCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}