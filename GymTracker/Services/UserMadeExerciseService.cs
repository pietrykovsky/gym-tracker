using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class UserMadeExerciseService : IUserMadeExerciseService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UserMadeExerciseService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<UserMadeExercise>> GetUserExercisesAsync(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserMadeExercise>> GetUserExercisesByCategoryAsync(string userId, int categoryId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.UserId == userId && e.Categories.Any(c => c.Id == categoryId))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserMadeExercise>> GetUserExercisesByDifficultyAsync(string userId, ExerciseDifficulty difficulty)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .Where(e => e.UserId == userId && e.Difficulty == difficulty)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<UserMadeExercise?> GetUserExerciseByIdAsync(string userId, int exerciseId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeExercises
            .AsNoTracking()
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == exerciseId);
    }

    public async Task<UserMadeExercise> CreateUserExerciseAsync(string userId, UserMadeExercise exercise, IEnumerable<int> categoryIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var categories = await context.ExerciseCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count == 0)
        {
            throw new ArgumentException("At least one valid category must be provided.");
        }

        exercise.UserId = userId;

        foreach (var category in categories)
        {
            exercise.Categories.Add(category);
        }

        await context.UserMadeExercises.AddAsync(exercise);
        await context.SaveChangesAsync();

        return exercise;
    }

    public async Task<UserMadeExercise?> UpdateUserExerciseAsync(string userId, int exerciseId, UserMadeExercise updatedExercise, IEnumerable<int> categoryIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var exercise = await context.UserMadeExercises
            .Include(e => e.PrimaryCategory)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == exerciseId);

        if (exercise == null)
            return null;

        var categories = await context.ExerciseCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count == 0)
        {
            throw new ArgumentException("At least one valid category must be provided.");
        }

        exercise.Name = updatedExercise.Name;
        exercise.Description = updatedExercise.Description;
        exercise.Difficulty = updatedExercise.Difficulty;
        exercise.RequiredEquipment = updatedExercise.RequiredEquipment;
        exercise.PrimaryCategoryId = updatedExercise.PrimaryCategoryId;
        exercise.Categories.Clear();

        foreach (var category in categories)
        {
            exercise.Categories.Add(category);
        }

        await context.SaveChangesAsync();
        return exercise;
    }

    public async Task<bool> DeleteUserExerciseAsync(string userId, int exerciseId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var exercise = await context.UserMadeExercises
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == exerciseId);

        if (exercise == null)
            return false;

        context.UserMadeExercises.Remove(exercise);
        await context.SaveChangesAsync();
        return true;
    }
}
