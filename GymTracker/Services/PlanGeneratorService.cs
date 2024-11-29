using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class PlanGeneratorService : IPlanGeneratorService
{
    private readonly IDefaultExerciseService _defaultExerciseService;
    private readonly IUserMadeExerciseService _userExerciseService;
    private readonly ITrainingPlanCategoryService _categoryService;

    public PlanGeneratorService(
        IDefaultExerciseService defaultExerciseService,
        IUserMadeExerciseService userExerciseService,
        ITrainingPlanCategoryService categoryService)
    {
        _defaultExerciseService = defaultExerciseService;
        _userExerciseService = userExerciseService;
        _categoryService = categoryService;
    }

    public WorkoutType GetWorkoutType(ExperienceLevel experience, int trainingDays) =>
        (experience, trainingDays) switch
        {
            // Untrained individuals benefit most from full body workouts (3 days per week optimal)
            (ExperienceLevel.Untrained, _) => WorkoutType.FullBody,

            // Trained individuals can handle split routines
            // With limited frequency (1-2 days), upper/lower split is more efficient
            (ExperienceLevel.Trained, <= 2) => WorkoutType.UpperLower,

            // For higher frequencies and advanced trainees, push/pull/legs allows more volume and better recovery
            // Split is superior
            _ => WorkoutType.PushPull
        };

    public async Task<UserMadeTrainingPlan> GenerateTrainingPlanAsync(
        string userId,
        TrainingGoal goal,
        ExperienceLevel experience,
        int trainingDays,
        IEnumerable<Equipment> availableEquipment,
        PushPullWorkoutDay? pushPullDay = null,
        UpperLowerWorkoutDay? upperLowerDay = null)
    {
        var allExercises = new List<ExerciseBase>();
        var userExercises = await _userExerciseService.GetUserExercisesAsync(userId);
        var defaultExercises = await _defaultExerciseService.GetAllExercisesAsync();

        allExercises.AddRange(userExercises);
        allExercises.AddRange(defaultExercises);

        var availableExercises = allExercises
            .Where(e => availableEquipment.Contains(e.RequiredEquipment))
            .ToList();

        var workoutType = GetWorkoutType(experience, trainingDays);
        var (sets, reps, rest) = GetTrainingParameters(goal, experience, trainingDays);

        var selectedExercises = workoutType switch
        {
            WorkoutType.FullBody => SelectExercisesForFullBody(availableExercises),
            WorkoutType.UpperLower when upperLowerDay.HasValue =>
                SelectExercisesForWorkout(availableExercises, upperLowerDay.Value, experience),
            WorkoutType.PushPull when pushPullDay.HasValue =>
                SelectExercisesForWorkout(availableExercises, pushPullDay.Value, experience),
            _ => throw new ArgumentException("Invalid workout configuration")
        };

        var activities = CreateActivities(selectedExercises, sets, reps, rest);
        var categories = await _categoryService.GetAllCategoriesAsync();
        var planCategories = GetPlanCategories(categories, goal, workoutType);

        var dayType = pushPullDay?.ToString() ?? upperLowerDay?.ToString() ?? "Full Body";

        return new UserMadeTrainingPlan
        {
            UserId = userId,
            Name = $"{dayType} {goal} Workout",
            Description = GenerateDescription(goal, experience, workoutType),
            Categories = planCategories,
            Activities = activities
        };
    }

    public void UpdatePlanWithWeights(
        UserMadeTrainingPlan plan,
        Dictionary<int, float> repMaxes,
        TrainingGoal goal,
        ExperienceLevel experience)
    {
        if (repMaxes.Count == 0)
        {
            return;
        }

        // Get 1RM percentage based on training parameters and research
        var (minPercentage, maxPercentage) = (goal, experience) switch
        {
            // Strength focus
            // Paper: 80-85%+ for advanced, progressive loading from 45-50% for untrained
            (TrainingGoal.Strength, ExperienceLevel.Untrained) => (0.45f, 0.50f), // 45-50% 1RM initially
            (TrainingGoal.Strength, ExperienceLevel.Trained) => (0.70f, 0.80f),   // 70-80% 1RM
            (TrainingGoal.Strength, ExperienceLevel.Advanced) => (0.80f, 0.90f),  // 80-90% 1RM

            // Hypertrophy focus
            // Paper: 6-12RM range optimal, moderate loads
            (TrainingGoal.Hypertrophy, ExperienceLevel.Untrained) => (0.60f, 0.65f), // Lighter loads initially
            (TrainingGoal.Hypertrophy, ExperienceLevel.Trained) => (0.65f, 0.75f),   // Moderate loads
            (TrainingGoal.Hypertrophy, ExperienceLevel.Advanced) => (0.70f, 0.80f),  // Higher end of range

            // Endurance focus 
            // Paper: Light loads with high reps
            (TrainingGoal.Endurance, ExperienceLevel.Untrained) => (0.45f, 0.50f), // Very light for beginners
            (TrainingGoal.Endurance, ExperienceLevel.Trained) => (0.50f, 0.60f),   // Light loads
            (TrainingGoal.Endurance, ExperienceLevel.Advanced) => (0.55f, 0.65f),  // Slightly higher with experience

            _ => (0.60f, 0.70f) // Default moderate range
        };

        var random = new Random();

        foreach (var activity in plan.Activities)
        {
            if (activity.Exercise?.RequiredEquipment == Equipment.None ||
                !repMaxes.TryGetValue(activity.ExerciseId, out var repMax) ||
                repMax <= 0 || activity.Exercise == null)
            {
                continue;
            }

            // Calculate the exercise complexity score
            var complexityScore = GetExerciseComplexityScore(activity.Exercise);
            
            // Adjust percentage based on exercise complexity
            // More complex exercises use slightly lower percentage of 1RM
            var complexity = complexityScore switch
            {
                > 100 => -0.05f,  // Compound movements
                > 50 => -0.025f,  // Intermediate complexity
                _ => 0f          // Isolation movements
            };

            // Generate a random percentage within the appropriate range
            var percentage = minPercentage + complexity + 
                (float)(random.NextDouble() * (maxPercentage - minPercentage));

            // Calculate initial recommended weight
            var baseWeight = RoundToNearest2Point5(repMax * percentage);

            // Progressive loading within sets if appropriate
            var setCount = activity.Sets.Count;
            for (var i = 0; i < setCount; i++)
            {
                var set = activity.Sets[i];
                
                if (goal == TrainingGoal.Strength && experience != ExperienceLevel.Untrained && setCount > 2)
                {
                    // Increase weight by 2.5kg or 5kg increments for each set after first
                    var increment = i * 2.5f;
                    set.Weight = RoundToNearest2Point5(baseWeight + increment);
                }
                else
                {
                    set.Weight = baseWeight;
                }
            }
        }
    }

    private static float RoundToNearest2Point5(float weight)
    {
        // Round down to nearest 2.5kg
        var roundedWeight = MathF.Floor(weight / 2.5f) * 2.5f;
        
        // Ensure minimum weight of 2.5kg
        return Math.Max(2.5f, roundedWeight);
    }

    private static (int sets, int reps, int rest) GetTrainingParameters(
        TrainingGoal goal, 
        ExperienceLevel experience,
        int trainingDays)
    {
        // First, get base parameters based on goal and experience
        var (baseSets, baseReps, baseRest) = (goal, experience) switch
        {
            // Strength focus
            (TrainingGoal.Strength, ExperienceLevel.Untrained) => (2, 8, 120),    // 45-50% 1RM initially
            (TrainingGoal.Strength, ExperienceLevel.Trained) => (3, 6, 180),      // 70-80% 1RM
            (TrainingGoal.Strength, ExperienceLevel.Advanced) => (4, 3, 300),     // 80-85%+ 1RM, 3-5 min rest

            // Hypertrophy focus
            (TrainingGoal.Hypertrophy, ExperienceLevel.Untrained) => (2, 10, 60),  // Lighter load initially
            (TrainingGoal.Hypertrophy, ExperienceLevel.Trained) => (3, 8, 90),     // 6-12 RM range
            (TrainingGoal.Hypertrophy, ExperienceLevel.Advanced) => (4, 6, 90),    // Higher intensity within range

            // Endurance focus
            (TrainingGoal.Endurance, ExperienceLevel.Untrained) => (2, 15, 45),   // Higher reps, shorter rest
            (TrainingGoal.Endurance, ExperienceLevel.Trained) => (3, 15, 30),     // Minimize rest as conditioning improves
            (TrainingGoal.Endurance, ExperienceLevel.Advanced) => (3, 20, 30),    // Increase volume through reps

            _ => throw new ArgumentException("Invalid training parameters")
        };

        // Adjust sets based on training frequency
        // Paper suggests higher frequency allows for lower per-session volume while maintaining weekly volume
        var adjustedSets = (trainingDays, experience) switch
        {
            // 1-2 days requires higher per-session volume to maintain adequate weekly volume
            (1, _) => baseSets + 2,                    // Add 2 sets to compensate for low frequency
            (2, _) => baseSets + 1,                    // Add 1 set to moderately compensate
            
            // 3 days is considered optimal for untrained/novice
            (3, ExperienceLevel.Untrained) => baseSets, // Keep base sets
            
            // 4-5 days allows for reduced per-session volume
            (4, _) => Math.Max(2, baseSets - 1),       // Reduce sets but maintain minimum of 2
            (5, _) => Math.Max(2, baseSets - 1),       // Reduce sets but maintain minimum of 2
            
            // 6 days requires careful management of volume
            (6, ExperienceLevel.Advanced) => Math.Max(2, baseSets - 2), // Further reduce sets for recovery
            (6, _) => Math.Max(2, baseSets - 1),       // Reduce sets but maintain minimum of 2
            
            // Default to base sets for any other combination
            _ => baseSets
        };

        // Adjust rest periods based on frequency
        // Higher frequency allows slightly shorter rest periods as per-session volume is lower
        var adjustedRest = (trainingDays, goal) switch
        {
            // Lower frequency needs full recovery between sets due to higher per-session volume
            (1 or 2, TrainingGoal.Strength) => baseRest,
            (1 or 2, _) => baseRest,
            
            // Moderate frequency can use standard rest periods
            (3 or 4, TrainingGoal.Strength) => baseRest,
            (3 or 4, _) => baseRest - 15, // Slightly reduce rest periods
            
            // Higher frequency can use shorter rest periods due to lower per-session volume
            (>= 5, TrainingGoal.Strength) => baseRest - 30,
            (>= 5, _) => baseRest - 30,
            
            _ => baseRest
        };

        // Reps remain constant as they're more related to the specific training goal
        // than to frequency

        return (adjustedSets, baseReps, adjustedRest);
    }

    private static List<ExerciseBase> SelectExercisesForFullBody(List<ExerciseBase> availableExercises)
    {
        // Prioritize compound movements based on research
        var exercises = new List<ExerciseBase>();

        // Core compound movements first
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Legs", 2)); // Squats, deadlifts
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Chest", 1)); // Bench variations
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Back", 2)); // Rows, pulldowns
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Shoulders", 1)); // Presses

        // Isolation movements
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Biceps", 1));
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Triceps", 1));
        exercises.AddRange(GetExercisesByCategory(availableExercises, "Core", 1));

        return exercises;
    }

    private static List<PlanActivity> CreateActivities(
        List<ExerciseBase> exercises,
        int setsPerExercise,
        int repsPerSet,
        int restPeriod)
    {
        var activities = new List<PlanActivity>();

        // Create activities for each exercise
        foreach (var exercise in exercises)
        {
            var activity = new PlanActivity
            {
                ExerciseId = exercise.Id,
                Exercise = exercise,
                Sets = new List<ExerciseSet>()
            };

            for (var setOrder = 1; setOrder <= setsPerExercise; setOrder++)
            {
                activity.Sets.Add(new ExerciseSet
                {
                    Order = setOrder,
                    Repetitions = repsPerSet,
                    RestAfterDuration = restPeriod
                });
            }

            activities.Add(activity);
        }

        // Sort activities by complexity score and assign order
        activities = activities
            .OrderByDescending(a => GetExerciseComplexityScore(a.Exercise!))
            .ToList();

        // Assign order after sorting
        for (int i = 0; i < activities.Count; i++)
        {
            activities[i].Order = i + 1;
        }

        return activities;
    }

    private static string GenerateDescription(TrainingGoal goal, ExperienceLevel experience, WorkoutType workoutType)
    {
        var focusDescription = goal switch
        {
            TrainingGoal.Strength => "strength development",
            TrainingGoal.Hypertrophy => "muscle growth",
            TrainingGoal.Endurance => "muscular endurance",
            _ => throw new ArgumentException("Invalid goal")
        };

        var typeDescription = workoutType switch
        {
            WorkoutType.FullBody => "full body training program targeting all major muscle groups in each session",
            WorkoutType.UpperLower => "split routine alternating between upper and lower body workouts",
            WorkoutType.PushPull => "split routine rotating between push, pull, and leg focused workouts",
            _ => throw new ArgumentException("Invalid workout type")
        };

        var levelDescription = experience switch
        {
            ExperienceLevel.Untrained => "beginners",
            ExperienceLevel.Trained => "intermediate trainees",
            ExperienceLevel.Advanced => "advanced trainees",
            _ => throw new ArgumentException("Invalid experience level")
        };

        return $"A {typeDescription} designed for {levelDescription}, focusing on {focusDescription}. " +
               $"Based on scientific research for optimal training adaptations.";
    }

    private static int GetExerciseComplexityScore(ExerciseBase exercise)
    {
        var score = 0;

        // Compound movements get higher priority
        if (exercise.GetMovementType() == MovementType.Compound)
            score += 100;

        // Add points for each muscle group involved
        if (exercise is DefaultExercise defaultEx)
            score += defaultEx.Categories.Count;
        else if (exercise is UserMadeExercise userEx)
            score += userEx.Categories.Count;

        return score;
    }

    private static List<ExerciseBase> SelectExercisesForWorkout(
        IEnumerable<ExerciseBase> availableExercises,
        PushPullWorkoutDay day,
        ExperienceLevel experience)
    {
        var exercises = new List<ExerciseBase>();
        var filtered = availableExercises.ToList();

        // Determine counts based on experience
        var compoundCount = experience switch
        {
            ExperienceLevel.Untrained => 3,
            ExperienceLevel.Trained => 4,
            ExperienceLevel.Advanced => 5,
            _ => 3
        };

        var isolationCount = experience switch
        {
            ExperienceLevel.Untrained => 1,
            ExperienceLevel.Trained => 2,
            ExperienceLevel.Advanced => 3,
            _ => 1
        };

        switch (day)
        {
            case PushPullWorkoutDay.Push:
                // Primary: Chest compound movements (bench variations, dips)
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Chest", compoundCount));

                // Supporting: Triceps isolation (pushdowns, extensions)
                exercises.AddRange(GetIsolationExercises(filtered, "Triceps", isolationCount));
                break;

            case PushPullWorkoutDay.Pull:
                // Primary: Back compound movements (rows, pulldowns)
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Back", compoundCount));

                // Supporting: Biceps isolation (curls)
                exercises.AddRange(GetIsolationExercises(filtered, "Biceps", isolationCount));
                break;

            case PushPullWorkoutDay.Legs:
                // Primary: Leg compound movements (squats, deadlifts)
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Legs", compoundCount));

                // Supporting: Core/isolation movements
                exercises.AddRange(GetIsolationExercises(filtered, "Core", isolationCount));
                break;
        }

        return exercises;
    }

    private static List<ExerciseBase> SelectExercisesForWorkout(
        IEnumerable<ExerciseBase> availableExercises,
        UpperLowerWorkoutDay day,
        ExperienceLevel experience)
    {
        var exercises = new List<ExerciseBase>();
        var filtered = availableExercises.ToList();

        var compoundCount = experience switch
        {
            ExperienceLevel.Untrained => 3,
            ExperienceLevel.Trained => 4,
            ExperienceLevel.Advanced => 5,
            _ => 3
        };

        var isolationCount = experience switch
        {
            ExperienceLevel.Untrained => 1,
            ExperienceLevel.Trained => 2,
            ExperienceLevel.Advanced => 2,
            _ => 1
        };

        switch (day)
        {
            case UpperLowerWorkoutDay.Upper:
                // Horizontal push/pull (chest/back)
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Chest", compoundCount / 2));
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Back", compoundCount / 2));

                // Vertical push/pull (shoulders/lats)
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Shoulders", 1));

                // Isolation for arms (synergistic muscles)
                exercises.AddRange(GetIsolationExercises(filtered, "Triceps", isolationCount));
                exercises.AddRange(GetIsolationExercises(filtered, "Biceps", isolationCount));
                break;

            case UpperLowerWorkoutDay.Lower:
                // Primary compound leg movements
                exercises.AddRange(GetPrimaryCompoundExercises(filtered, "Legs", compoundCount));

                // Core/stabilizers
                exercises.AddRange(GetIsolationExercises(filtered, "Core", isolationCount));
                exercises.AddRange(GetIsolationExercises(filtered, "Glutes", 1));
                break;
        }

        return exercises;
    }

    private static List<ExerciseBase> GetPrimaryCompoundExercises(
        List<ExerciseBase> exercises,
        string primaryCategory,
        int count)
    {
        return exercises
            .Where(e =>
                e.GetMovementType() == MovementType.Compound &&
                e.PrimaryCategory.Name == primaryCategory)
            .OrderByDescending(GetExerciseComplexityScore)
            .Take(count)
            .ToList();
    }

    private static List<ExerciseBase> GetIsolationExercises(
        List<ExerciseBase> exercises,
        string targetCategory,
        int count)
    {
        return exercises
            .Where(e =>
                e.GetMovementType() == MovementType.Isolation &&
                (e.PrimaryCategory.Name == targetCategory ||
                 (e is DefaultExercise defaultEx && defaultEx.Categories.Any(c => c.Name == targetCategory)) ||
                 (e is UserMadeExercise userEx && userEx.Categories.Any(c => c.Name == targetCategory))))
            .OrderByDescending(GetExerciseComplexityScore)
            .Take(count)
            .ToList();
    }

    private static List<ExerciseBase> GetExercisesByCategory(
        List<ExerciseBase> exercises,
        string category,
        int count)
    {
        var matching = exercises
            .Where(e =>
                (e is DefaultExercise defaultEx &&
                 (defaultEx.PrimaryCategory.Name == category)) ||
                (e is UserMadeExercise userEx &&
                 (userEx.PrimaryCategory.Name == category)))
            .OrderByDescending(GetExerciseComplexityScore)
            .Take(count)
            .ToList();

        // If we don't have enough exercises, just return what we have
        return matching;
    }

    private static List<TrainingPlanCategory> GetPlanCategories(
        IEnumerable<TrainingPlanCategory> allCategories,
        TrainingGoal goal,
        WorkoutType workoutType)
    {
        var categories = new List<TrainingPlanCategory>();

        // Add workout type category
        categories.AddRange(workoutType switch
        {
            WorkoutType.FullBody => allCategories.Where(c => c.Name == "Full Body"),
            WorkoutType.UpperLower => allCategories.Where(c => c.Name is "Upper/Lower"),
            WorkoutType.PushPull => allCategories.Where(c => c.Name == "Split Routine"),
            _ => Enumerable.Empty<TrainingPlanCategory>()
        });

        // Add goal-based category
        categories.AddRange(goal switch
        {
            TrainingGoal.Strength => allCategories.Where(c => c.Name == "Strength"),
            TrainingGoal.Endurance => allCategories.Where(c =>
                c.Name is "Endurance"),
            _ => Enumerable.Empty<TrainingPlanCategory>()
        });

        return categories.Distinct().ToList();
    }
}