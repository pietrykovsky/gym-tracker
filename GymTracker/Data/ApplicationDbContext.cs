using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public virtual DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
    public virtual DbSet<ExerciseCategory> ExerciseCategories => Set<ExerciseCategory>();
    public virtual DbSet<DefaultExercise> DefaultExercises => Set<DefaultExercise>();
    public virtual DbSet<UserMadeExercise> UserMadeExercises => Set<UserMadeExercise>();
}
