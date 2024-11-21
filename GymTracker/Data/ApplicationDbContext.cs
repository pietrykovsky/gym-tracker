using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public virtual DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
    public virtual DbSet<ExerciseCategory> ExerciseCategories => Set<ExerciseCategory>();
    public virtual DbSet<DefaultExercise> DefaultExercises => Set<DefaultExercise>();
    public virtual DbSet<UserMadeExercise> UserMadeExercises => Set<UserMadeExercise>();
    public virtual DbSet<TrainingPlanCategory> TrainingPlanCategories => Set<TrainingPlanCategory>();
    public virtual DbSet<DefaultTrainingPlan> DefaultTrainingPlans => Set<DefaultTrainingPlan>();
    public virtual DbSet<UserMadeTrainingPlan> UserMadeTrainingPlans => Set<UserMadeTrainingPlan>();
    public virtual DbSet<PlanActivity> PlanActivities => Set<PlanActivity>();
    public virtual DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();
    public virtual DbSet<TrainingActivity> TrainingActivities => Set<TrainingActivity>();
    public virtual DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
}
