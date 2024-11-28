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
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DefaultExercise>> GetAllExercisesByCategoryAsync(int categoryId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.Categories.Any(c => c.Id == categoryId))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<DefaultExercise?> GetExerciseByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<DefaultExercise>> GetExercisesByDifficultyAsync(ExerciseDifficulty difficulty)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.Difficulty == difficulty)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DefaultExercise>> GetExercisesByMaxDifficultyAsync(ExerciseDifficulty maxDifficulty)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.Difficulty <= maxDifficulty)
            .OrderBy(e => e.Difficulty)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DefaultExercise>> GetExercisesByDifficultyRangeAsync(ExerciseDifficulty minDifficulty, ExerciseDifficulty maxDifficulty)
    {
        if (minDifficulty > maxDifficulty)
        {
            throw new ArgumentException("Minimum difficulty cannot be greater than maximum difficulty.");
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.DefaultExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.Difficulty >= minDifficulty && e.Difficulty <= maxDifficulty)
            .OrderBy(e => e.Difficulty)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }
}