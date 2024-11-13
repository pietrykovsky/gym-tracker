using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class DefaultExerciseService : IDefaultExerciseService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public DefaultExerciseService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<DefaultExercise>> GetAllExercisesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.Category)
            .OrderBy(e => e.Category.Name)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DefaultExercise>> GetAllExercisesByCategoryAsync(int categoryId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Where(e => e.CategoryId == categoryId)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<DefaultExercise?> GetExerciseByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
