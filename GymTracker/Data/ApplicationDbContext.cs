using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public virtual DbSet<BodyMeasurement> BodyMeasurements { get; set; }
    public virtual DbSet<ExerciseCategory> ExerciseCategories { get; set; }
    public virtual DbSet<DefaultExercise> DefaultExercises { get; set; }
    public virtual DbSet<UserMadeExercise> UserMadeExercises { get; set; }
}
