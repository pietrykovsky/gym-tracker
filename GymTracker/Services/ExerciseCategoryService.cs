using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class ExerciseCategoryService : IExerciseCategoryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ExerciseCategoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<ExerciseCategory>> GetAllCategoriesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ExerciseCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ExerciseCategory?> GetCategoryByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ExerciseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ExerciseCategory?> GetCategoryWithExercisesAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ExerciseCategories
            .AsNoTracking()
            .Include(c => c.DefaultExercises)
            .Include(c => c.UserMadeExercises)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
