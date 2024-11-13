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
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Category.Name)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserMadeExercise>> GetUserExercisesByCategoryAsync(string userId, int categoryId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeExercises
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.CategoryId == categoryId)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<UserMadeExercise?> GetUserExerciseByIdAsync(string userId, int exerciseId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserMadeExercises
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == exerciseId);
    }

    public async Task<UserMadeExercise> CreateUserExerciseAsync(string userId, UserMadeExercise exercise)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        exercise.UserId = userId;
        await context.UserMadeExercises.AddAsync(exercise);
        await context.SaveChangesAsync();
        return exercise;
    }

    public async Task<UserMadeExercise?> UpdateUserExerciseAsync(string userId, int exerciseId, UserMadeExercise updatedExercise)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var exercise = await context.UserMadeExercises
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == exerciseId);

        if (exercise == null)
            return null;

        exercise.Name = updatedExercise.Name;
        exercise.Description = updatedExercise.Description;
        exercise.CategoryId = updatedExercise.CategoryId;

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
