using GymTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Services;

public class DatabaseSeederService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<DatabaseSeederService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await SeedCategoriesAsync(context);
            await SeedDefaultExercisesAsync(context);
            await SeedTrainingPlanCategoriesAsync(context);
            await SeedDefaultTrainingPlansAsync(context);
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        foreach (var category in muscleGroups)
        {
            if (!await context.ExerciseCategories.AnyAsync(c => c.Name == category.Name))
            {
                await context.ExerciseCategories.AddAsync(new ExerciseCategory
                {
                    Name = category.Name,
                    Description = category.Description
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultExercisesAsync(ApplicationDbContext context)
    {
        var categories = await context.ExerciseCategories.ToDictionaryAsync(c => c.Name, c => c);

        foreach (var exercise in defaultExercises)
        {
            if (!await context.DefaultExercises.AnyAsync(e => e.Name == exercise.Name))
            {
                await context.DefaultExercises.AddAsync(new DefaultExercise
                {
                    Name = exercise.Name,
                    Description = exercise.Description,
                    Difficulty = exercise.Difficulty,
                    RequiredEquipment = exercise.RequiredEquipment,
                    PrimaryCategory = categories.FirstOrDefault(c => c.Key == exercise.PrimaryCategory).Value,
                    Categories = exercise.Categories.Select(c => categories[c]).ToList()
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedTrainingPlanCategoriesAsync(ApplicationDbContext context)
    {
        foreach (var category in trainingPlanCategories)
        {
            if (!await context.TrainingPlanCategories.AnyAsync(c => c.Name == category.Name))
            {
                await context.TrainingPlanCategories.AddAsync(new TrainingPlanCategory
                {
                    Name = category.Name,
                    Description = category.Description
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultTrainingPlansAsync(ApplicationDbContext context)
    {
        var categories = await context.TrainingPlanCategories.ToDictionaryAsync(c => c.Name, c => c);
        var exercises = await context.DefaultExercises.ToDictionaryAsync(e => e.Name, e => e);

        foreach (var plan in defaultTrainingPlans)
        {
            if (!await context.DefaultTrainingPlans.AnyAsync(p => p.Name == plan.Name))
            {
                var newPlan = new DefaultTrainingPlan
                {
                    Name = plan.Name,
                    Description = plan.Description,
                    Categories = plan.Categories.Select(c => categories[c]).ToList()
                };

                await context.DefaultTrainingPlans.AddAsync(newPlan);
                await context.SaveChangesAsync();

                var activities = new List<PlanActivity>();
                var order = 1;

                foreach (var activity in plan.Activities)
                {
                    if (!exercises.ContainsKey(activity.ExerciseName))
                        continue;

                    var newActivity = new PlanActivity
                    {
                        PlanId = newPlan.Id,
                        ExerciseId = exercises[activity.ExerciseName].Id,
                        Order = order++,
                        Sets = activity.Sets.Select((set, index) => new ExerciseSet
                        {
                            Order = index + 1,
                            Repetitions = set.Repetitions,
                            Weight = set.Weight,
                            RestAfterDuration = set.RestAfterDuration
                        }).ToList()
                    };

                    activities.Add(newActivity);
                }

                await context.PlanActivities.AddRangeAsync(activities);
            }
        }

        await context.SaveChangesAsync();
    }

    private record Category(string Name, string Description);
    private record Exercise(string Name, string Description, ExerciseDifficulty Difficulty, Equipment RequiredEquipment, string PrimaryCategory, IEnumerable<string> Categories);
    private record Set(int Repetitions, float? Weight = null, int? RestAfterDuration = 60);
    private record Activity(string ExerciseName, IEnumerable<Set> Sets);
    private record Plan(string Name, string Description, IEnumerable<string> Categories, IEnumerable<Activity> Activities);

    private static List<Category> muscleGroups = new()
    {
        new("Chest", "Exercises targeting the pectoralis major and minor."),
        new("Back", "Exercises for back muscles including latissimus dorsi, trapezius, rhomboids, and erector spinae."),
        new("Legs", "Lower body exercises targeting quadriceps, hamstrings, glutes, and calves."),
        new("Shoulders", "Exercises for deltoid muscles and rotator cuff."),
        new("Biceps", "Isolation and compound exercises for biceps brachii and brachialis."),
        new("Triceps", "Exercises targeting the triceps brachii."),
        new("Forearms", "Exercises focusing on wrist flexors, extensors, and grip strength."),
        new("Core", "Exercises for abdominal muscles, obliques, and transverse abdominis."),
        new("Glutes", "Exercises for gluteus maximus, medius, and minimus."),
        new("Calves", "Exercises targeting the gastrocnemius and soleus muscles."),
        new("Obliques", "Specific exercises for internal and external obliques."),
        new("Lats", "Exercises isolating latissimus dorsi."),
        new("Traps", "Exercises focusing on the trapezius muscle.")
    };

    private static List<Exercise> defaultExercises = new()
    {
        // Chest exercises
        new("Flat Barbell Bench Press", "Lie on a flat bench, grip barbell slightly wider than shoulder width, lower to chest and press back up while maintaining proper back arch and foot position", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Chest", ["Triceps", "Shoulders"]),
        new("Incline Dumbbell Press", "On an incline bench (30-45 degrees), press dumbbells from shoulder level to full extension, targeting upper chest", ExerciseDifficulty.Intermediate, Equipment.Dumbbell, "Chest", ["Triceps", "Shoulders"]),
        new("Decline Bench Press", "Perform bench press on a decline bench to target lower chest fibers", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Chest", ["Triceps", "Shoulders"]),
        new("Dumbbell Flyes", "Lie flat holding dumbbells above chest, lower weights in wide arc maintaining slight elbow bend, focus on chest stretch", ExerciseDifficulty.Intermediate, Equipment.Dumbbell, "Chest", []),
        new("Push-Ups", "Standard push-up position, hands slightly wider than shoulders, lower body until chest nearly touches ground", ExerciseDifficulty.Beginner, Equipment.None, "Chest", ["Triceps", "Core"]),
        new("Cable Crossovers", "Stand between cable machines, perform crossing motion with arms from high to low position", ExerciseDifficulty.Novice, Equipment.Cable, "Chest", []),
        new("Chest Dips", "Support body on parallel bars, lean forward slightly, lower body until chest stretch is felt, then press back up", ExerciseDifficulty.Intermediate, Equipment.None, "Chest", ["Triceps"]),
        new("Machine Chest Press", "Sit at chest press machine, grip handles at chest level, press forward until arms are extended", ExerciseDifficulty.Beginner, Equipment.Machine, "Chest", ["Triceps"]),
        new("Landmine Press", "Hold barbell end at shoulder, press up and forward in an arc motion", ExerciseDifficulty.Novice, Equipment.Barbell, "Chest", ["Shoulders"]),
        new("Smith Machine Bench Press", "Perform bench press using Smith machine for guided movement pattern", ExerciseDifficulty.Novice, Equipment.Machine, "Chest", ["Triceps", "Shoulders"]),
        new("Incline Barbell Bench Press", "Bench press performed on an incline bench to target upper chest", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Chest", ["Triceps", "Shoulders"]),
        new("Decline Dumbbell Press", "Press dumbbells while lying on a decline bench", ExerciseDifficulty.Intermediate, Equipment.Dumbbell, "Chest", ["Triceps", "Shoulders"]),

        // Back exercises
        new("Deadlift", "Stand with feet hip-width, bend at hips and knees to grip barbell, maintain neutral spine while lifting bar to hip level", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Glutes", "Legs", "Core", "Traps"]),
        new("Pull-Ups", "Hang from bar with wide grip, pull body up until chin clears bar, focus on engaging lats", ExerciseDifficulty.Intermediate, Equipment.None, "Back", ["Biceps", "Core"]),
        new("Barbell Rows", "Bend forward at hips holding barbell, pull weight to lower chest while keeping back straight", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Back", ["Biceps", "Core"]),
        new("Seated Cable Rows", "Sit at cable machine, pull handle to torso while maintaining upright posture", ExerciseDifficulty.Beginner, Equipment.Cable, "Back", ["Biceps"]),
        new("Face Pulls", "Use rope attachment on cable machine at head height, pull towards face while separating rope ends", ExerciseDifficulty.Novice, Equipment.Cable, "Back", ["Shoulders", "Traps"]),
        new("Single-Arm Dumbbell Row", "One knee and hand on bench, row dumbbell to hip while keeping back straight", ExerciseDifficulty.Novice, Equipment.Dumbbell, "Back", ["Biceps", "Core"]),
        new("Meadows Row", "Perform single-arm rows with barbell from landmine attachment", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Back", ["Biceps"]),
        new("Straight-Arm Pulldown", "Use cable machine, keep arms straight while pulling bar down to thighs", ExerciseDifficulty.Novice, Equipment.Cable, "Back", ["Triceps"]),
        new("T-Bar Row", "Straddle T-bar machine or barbell, pull weight up towards chest", ExerciseDifficulty.Intermediate, Equipment.Machine, "Back", ["Biceps", "Core"]),
        new("Rack Pulls", "Partial deadlift movement from elevated starting position", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Glutes", "Legs"]),
        new("Chin-Ups", "Pull-ups with underhand grip, targeting biceps more", ExerciseDifficulty.Intermediate, Equipment.None, "Back", ["Biceps", "Core"]),
        new("Neutral Grip Pull-Ups", "Pull-ups using parallel grip handles", ExerciseDifficulty.Intermediate, Equipment.None, "Back", ["Biceps", "Core"]),
        new("Pendlay Row", "Barbell row starting each rep from the floor", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Biceps", "Core"]),
        new("Lat Pulldown", "Cable pulldown exercise targeting latissimus dorsi", ExerciseDifficulty.Beginner, Equipment.Cable, "Back", ["Biceps"]),

        // Legs exercises
        new("Back Squat", "Barbell across upper back, feet shoulder-width, squat down keeping chest up and knees tracking over toes", ExerciseDifficulty.Advanced, Equipment.Barbell, "Legs", ["Glutes", "Core"]),
        new("Front Squat", "Barbell in front rack position, perform squat maintaining upright torso", ExerciseDifficulty.Advanced, Equipment.Barbell, "Legs", ["Glutes", "Core"]),
        new("Romanian Deadlift", "Hold barbell at hips, hinge forward maintaining slight knee bend and flat back", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Legs", ["Back", "Glutes"]),
        new("Walking Lunges", "Step forward into lunge position, lower back knee toward ground, alternate legs", ExerciseDifficulty.Novice, Equipment.None, "Legs", ["Glutes", "Core"]),
        new("Bulgarian Split Squats", "Rear foot elevated on bench, perform single-leg squat with front leg", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Glutes", "Core"]),
        new("Leg Press", "Sit in machine with feet shoulder-width, press weight while maintaining lower back contact", ExerciseDifficulty.Beginner, Equipment.Machine, "Legs", ["Glutes"]),
        new("Standing Calf Raises", "Stand on edge of platform, raise heels up and lower back down slowly", ExerciseDifficulty.Beginner, Equipment.Machine, "Calves", []),
        new("Seated Calf Raises", "Perform calf raises while seated with weight on knees", ExerciseDifficulty.Beginner, Equipment.Machine, "Calves", []),
        new("Hack Squat", "Use hack squat machine, position shoulders and back against pad, squat down", ExerciseDifficulty.Intermediate, Equipment.Machine, "Legs", ["Glutes"]),
        new("Good Mornings", "Barbell on upper back, hinge at hips while maintaining slight knee bend", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Back", ["Legs", "Glutes"]),
        new("Leg Extensions", "Sit in machine, extend legs to straight position, focusing on quad contraction", ExerciseDifficulty.Beginner, Equipment.Machine, "Legs", []),
        new("Lying Leg Curls", "Lie face down on machine, curl weight using hamstrings", ExerciseDifficulty.Beginner, Equipment.Machine, "Legs", []),
        new("Sissy Squats", "Hold support, lean back and bend knees while keeping hips straight", ExerciseDifficulty.Advanced, Equipment.None, "Legs", ["Core"]),
        new("Box Squats", "Perform squat to box, pause, then drive up", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Legs", ["Glutes", "Core"]),
        new("Step-Ups", "Step up onto elevated platform, driving through heel", ExerciseDifficulty.Novice, Equipment.None, "Legs", ["Glutes"]),
        new("Glute Bridge", "Lie on back, drive hips up while squeezing glutes", ExerciseDifficulty.Beginner, Equipment.None, "Glutes", ["Legs"]),
        new("Hip Thrust", "Rest upper back on bench, drive hips up with weight on lap", ExerciseDifficulty.Novice, Equipment.Barbell, "Glutes", ["Legs"]),

        // Shoulders exercises
        new("Overhead Press", "Stand holding barbell at shoulders, press overhead while maintaining stable core", ExerciseDifficulty.Advanced, Equipment.Barbell, "Shoulders", ["Triceps", "Core"]),
        new("Lateral Raises", "Stand holding dumbbells at sides, raise arms out to shoulder level maintaining slight bend", ExerciseDifficulty.Beginner, Equipment.Dumbbell, "Shoulders", []),
        new("Front Raises", "Hold weights in front of thighs, raise to shoulder height keeping arms straight", ExerciseDifficulty.Beginner, Equipment.Dumbbell, "Shoulders", []),
        new("Reverse Flyes", "Bend forward holding dumbbells, raise arms out to sides focusing on rear deltoids", ExerciseDifficulty.Novice, Equipment.Dumbbell, "Shoulders", ["Traps"]),
        new("Arnold Press", "Seated press starting with palms facing you, rotate to palms forward while pressing up", ExerciseDifficulty.Intermediate, Equipment.Dumbbell, "Shoulders", ["Triceps"]),
        new("Military Press", "Strict overhead press with feet together, maintaining rigid body position", ExerciseDifficulty.Advanced, Equipment.Barbell, "Shoulders", ["Triceps", "Core"]),
        new("Behind the Neck Press", "Press barbell from behind neck position to overhead", ExerciseDifficulty.Advanced, Equipment.Barbell, "Shoulders", ["Triceps"]),
        new("Cable Lateral Raises", "Perform lateral raises using low cable pulley", ExerciseDifficulty.Novice, Equipment.Cable, "Shoulders", []),
        new("Upright Rows", "Pull barbell or dumbbells vertically along body to chin height", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Shoulders", ["Traps"]),
        new("Plate Front Raises", "Hold weight plate with arms extended, raise to shoulder height", ExerciseDifficulty.Novice, Equipment.Barbell, "Shoulders", []),
        new("Face Pull with External Rotation", "Face pull variation emphasizing rotator cuff engagement", ExerciseDifficulty.Novice, Equipment.Cable, "Shoulders", ["Traps"]),
        new("Landmine Lateral Raises", "One-arm lateral raise using landmine attachment", ExerciseDifficulty.Novice, Equipment.Barbell, "Shoulders", []),

        // Arms exercises
        new("Barbell Curl", "Stand holding barbell with underhand grip, curl weight while keeping upper arms static", ExerciseDifficulty.Novice, Equipment.Barbell, "Biceps", ["Forearms"]),
        new("Skull Crushers", "Lie on bench holding weight above forehead, lower behind head by bending elbows", ExerciseDifficulty.Novice, Equipment.Barbell, "Triceps", []),
        new("Hammer Curls", "Perform bicep curls with neutral grip (palms facing each other)", ExerciseDifficulty.Beginner, Equipment.Dumbbell, "Biceps", ["Forearms"]),
        new("Tricep Pushdowns", "Use cable machine with straight or rope attachment, extend arms downward", ExerciseDifficulty.Beginner, Equipment.Cable, "Triceps", []),
        new("Preacher Curls", "Perform bicep curls with arms supported on preacher bench", ExerciseDifficulty.Novice, Equipment.Barbell, "Biceps", []),
        new("Diamond Push-Ups", "Push-ups with hands close together forming diamond shape, targeting triceps", ExerciseDifficulty.Intermediate, Equipment.None, "Triceps", ["Chest"]),
        new("Tricep Dips", "Support body on parallel bars, keep torso vertical, bend and extend arms", ExerciseDifficulty.Intermediate, Equipment.None, "Triceps", ["Chest"]),
        new("Concentration Curls", "Seated single-arm curl with elbow braced against inner thigh", ExerciseDifficulty.Beginner, Equipment.Dumbbell, "Biceps", []),
        new("Overhead Tricep Extension", "Hold weight overhead, lower behind head by bending elbows", ExerciseDifficulty.Novice, Equipment.Dumbbell, "Triceps", []),
        new("21s", "Perform bicep curls in three ranges of motion, 7 reps each", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Biceps", ["Forearms"]),
        new("Reverse Curls", "Curl weight using overhand grip to target forearms", ExerciseDifficulty.Novice, Equipment.Barbell, "Forearms", ["Biceps"]),
        new("Close-Grip Bench Press", "Perform bench press with narrow hand spacing for triceps", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Triceps", ["Chest"]),
        new("Spider Curls", "Perform curls lying face down on incline bench", ExerciseDifficulty.Novice, Equipment.Dumbbell, "Biceps", []),
        new("Zottman Curls", "Curl up with supinated grip, rotate to pronated grip for lowering", ExerciseDifficulty.Intermediate, Equipment.Dumbbell, "Biceps", ["Forearms"]),
        new("Cable Hammer Curls", "Perform hammer curls using cable machine", ExerciseDifficulty.Beginner, Equipment.Cable, "Biceps", ["Forearms"]),
        new("JM Press", "Hybrid of close-grip bench press and skull crusher", ExerciseDifficulty.Advanced, Equipment.Barbell, "Triceps", ["Chest"]),

        // Core exercises
        new("Plank", "Hold push-up position on forearms, maintaining straight body alignment", ExerciseDifficulty.Beginner, Equipment.None, "Core", []),
        new("Russian Twists", "Seated with feet off ground, rotate torso side to side holding weight", ExerciseDifficulty.Novice, Equipment.None, "Core", ["Obliques"]),
        new("Cable Woodchops", "Stand sideways to cable machine, rotate torso pulling weight across body", ExerciseDifficulty.Novice, Equipment.Cable, "Obliques", ["Core"]),
        new("Hanging Leg Raises", "Hang from pull-up bar, raise legs to parallel while keeping them straight", ExerciseDifficulty.Advanced, Equipment.None, "Core", ["Obliques"]),
        new("Ab Wheel Rollouts", "Kneel holding ab wheel, roll forward and back maintaining core tension", ExerciseDifficulty.Advanced, Equipment.None, "Core", []),
        new("Dragon Flag", "Advanced movement keeping body straight while rolling up and down bench", ExerciseDifficulty.Expert, Equipment.None, "Core", ["Back"]),
        new("Cable Crunches", "Kneel at cable machine, crunch down pulling rope attachment", ExerciseDifficulty.Novice, Equipment.Cable, "Core", []),
        new("Windshield Wipers", "Hang from pull-up bar, swing legs side to side while keeping straight", ExerciseDifficulty.Expert, Equipment.None, "Core", ["Obliques"]),
        new("Copenhagen Plank", "Side plank with top leg supported, bottom leg raised", ExerciseDifficulty.Advanced, Equipment.None, "Core", ["Obliques"]),
        new("Pallof Press", "Anti-rotation exercise using cable machine", ExerciseDifficulty.Intermediate, Equipment.Cable, "Core", ["Obliques"]),
        new("Turkish Get-Up", "Complex movement from lying to standing while holding weight overhead", ExerciseDifficulty.Advanced, Equipment.Kettlebell, "Core", ["Shoulders", "Legs", "Back"]),
        new("Dead Bug", "Lie on back, alternately extend opposite arm and leg", ExerciseDifficulty.Beginner, Equipment.None, "Core", []),
        new("Bird Dog", "Quadruped position, extend opposite arm and leg", ExerciseDifficulty.Beginner, Equipment.None, "Core", ["Back"]),
        new("Side Plank", "Balance on forearm and side of feet, maintain straight body", ExerciseDifficulty.Novice, Equipment.None, "Core", ["Obliques"]),

        // Cardio exercises
        new("High-Intensity Interval Training", "Alternate between intense exercise and rest periods", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Core", "Back"]),
        new("Rowing", "Use rowing machine maintaining proper form through drive and recovery phases", ExerciseDifficulty.Novice, Equipment.Machine, "Back", ["Legs", "Core", "Biceps"]),
        new("Jump Rope", "Basic bouncing or advanced variations for cardio development", ExerciseDifficulty.Novice, Equipment.None, "Calves", ["Legs"]),
        new("Stair Climber", "Use stair climbing machine maintaining upright posture", ExerciseDifficulty.Beginner, Equipment.Machine, "Legs", ["Glutes", "Calves"]),
        new("Sprint Intervals", "Alternating between sprinting and jogging/walking", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Calves", "Glutes"]),
        new("Cycling", "Indoor or outdoor cycling for cardiovascular endurance", ExerciseDifficulty.Beginner, Equipment.Machine, "Legs", ["Glutes"]),
        new("Mountain Climbers", "Plank position, alternately drive knees towards chest", ExerciseDifficulty.Novice, Equipment.None, "Core", ["Legs"]),
        new("Battle Ropes", "Create waves or slams with heavy ropes", ExerciseDifficulty.Intermediate, Equipment.None, "Shoulders", ["Core", "Back"]),

        // Plyometrics
        new("Box Jumps", "Explosive jump onto raised platform, step back down", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Glutes", "Calves"]),
        new("Jump Squats", "Perform squat with explosive jump at top of movement", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Glutes", "Core"]),
        new("Depth Jumps", "Step off box, land and immediately jump vertically", ExerciseDifficulty.Advanced, Equipment.None, "Legs", ["Glutes", "Calves"]),
        new("Plyo Push-Ups", "Explosive push-ups with hands leaving ground", ExerciseDifficulty.Advanced, Equipment.None, "Chest", ["Triceps", "Core"]),
        new("Broad Jumps", "Horizontal jumping for distance", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Glutes", "Core"]),
        new("Bounding", "Exaggerated running steps for distance", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Glutes", "Calves"]),
        new("Medicine Ball Slams", "Explosively slam medicine ball to ground", ExerciseDifficulty.Novice, Equipment.None, "Core", ["Shoulders", "Back"]),

        // Olympic lifts
        new("Clean and Jerk", "Two-part Olympic lift: explosive pull to shoulders, then drive overhead", ExerciseDifficulty.Expert, Equipment.Barbell, "Legs", ["Back", "Shoulders", "Core", "Traps"]),
        new("Snatch", "Single explosive movement from ground to overhead with wide grip", ExerciseDifficulty.Expert, Equipment.Barbell, "Legs", ["Back", "Shoulders", "Core", "Traps"]),
        new("Power Clean", "Clean variation caught in partial squat position", ExerciseDifficulty.Advanced, Equipment.Barbell, "Legs", ["Back", "Shoulders", "Traps"]),
        new("Hang Clean", "Clean starting from hanging position at thighs", ExerciseDifficulty.Advanced, Equipment.Barbell, "Legs", ["Back", "Shoulders", "Traps"]),
        new("Clean Pull", "Clean movement without catching the bar", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Legs", "Traps"]),
        new("Snatch Pull", "Snatch movement without catching the bar", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Legs", "Traps"]),
        new("Power Snatch", "Snatch caught in partial squat position", ExerciseDifficulty.Expert, Equipment.Barbell, "Legs", ["Back", "Shoulders", "Traps"]),
        new("Hang Snatch", "Snatch starting from hanging position", ExerciseDifficulty.Expert, Equipment.Barbell, "Legs", ["Back", "Shoulders", "Traps"]),
        new("Split Jerk", "Drive weight overhead with split leg position", ExerciseDifficulty.Advanced, Equipment.Barbell, "Legs", ["Shoulders", "Core"]),
        new("Push Press", "Overhead press with leg drive assistance", ExerciseDifficulty.Intermediate, Equipment.Barbell, "Shoulders", ["Legs", "Triceps"]),
        new("Muscle Snatch", "Snatch performed without dropping under bar", ExerciseDifficulty.Expert, Equipment.Barbell, "Shoulders", ["Back", "Traps"]),

        // Functional
        new("Farmer's Walks", "Carry heavy dumbbells or kettlebells while walking", ExerciseDifficulty.Novice, Equipment.Dumbbell, "Back", ["Core", "Traps", "Forearms"]),
        new("Sled Push/Pull", "Push or pull weighted sled across floor", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Core", "Back"]),
        new("Sandbag Carry", "Carry heavy sandbag in various positions", ExerciseDifficulty.Intermediate, Equipment.None, "Back", ["Core", "Legs"]),
        new("Tire Flips", "Flip large tire using hip hinge and explosive movement", ExerciseDifficulty.Advanced, Equipment.None, "Back", ["Legs", "Core"]),
        new("Prowler Push", "Push weighted sled focusing on leg drive", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Core", "Shoulders"]),
        new("Racked Carries", "Carry kettlebells/dumbbells in front rack position", ExerciseDifficulty.Intermediate, Equipment.Kettlebell, "Core", ["Back", "Shoulders"]),
        new("Overhead Carries", "Walk while holding weight overhead", ExerciseDifficulty.Advanced, Equipment.Dumbbell, "Shoulders", ["Core", "Traps"]),
        new("Zercher Carry", "Carry barbell in crook of elbows", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Core", "Biceps"]),
        new("Yoke Walk", "Walk with heavy yoke across shoulders", ExerciseDifficulty.Expert, Equipment.None, "Back", ["Legs", "Core", "Traps"]),

        // Calisthenics
        new("Muscle-Ups", "Advanced pull-up transitioning to dip position above bar", ExerciseDifficulty.Expert, Equipment.None, "Back", ["Chest", "Triceps", "Core"]),
        new("Handstand Push-Ups", "Inverted push-ups against wall for shoulder strength", ExerciseDifficulty.Expert, Equipment.None, "Shoulders", ["Triceps", "Core"]),
        new("L-Sits", "Hold body above ground with legs extended horizontally", ExerciseDifficulty.Advanced, Equipment.None, "Core", ["Shoulders", "Triceps"]),
        new("Front Lever", "Hold body parallel to ground while hanging", ExerciseDifficulty.Expert, Equipment.None, "Back", ["Core", "Lats"]),
        new("Back Lever", "Hold body parallel to ground facing upward", ExerciseDifficulty.Expert, Equipment.None, "Back", ["Core", "Chest"]),
        new("Planche", "Hold body parallel to ground on hands only", ExerciseDifficulty.Expert, Equipment.None, "Shoulders", ["Chest", "Core"]),
        new("Human Flag", "Hold body horizontally while gripping vertical pole", ExerciseDifficulty.Expert, Equipment.None, "Core", ["Back", "Shoulders"]),
        new("Pistol Squats", "Single-leg squats with other leg extended", ExerciseDifficulty.Advanced, Equipment.None, "Legs", ["Core", "Glutes"]),
        new("Ring Muscle-Ups", "Muscle-ups performed on gymnastic rings", ExerciseDifficulty.Expert, Equipment.None, "Back", ["Chest", "Triceps", "Core"]),
        new("Archer Pull-Ups", "Pull-ups with one arm more extended than other", ExerciseDifficulty.Advanced, Equipment.None, "Back", ["Biceps", "Core"]),

        // Recovery/Mobility
        new("Foam Rolling", "Self-myofascial release using foam roller on tight muscles", ExerciseDifficulty.Beginner, Equipment.None, "Back", []),
        new("Light Jogging", "Easy-paced jogging for active recovery", ExerciseDifficulty.Beginner, Equipment.None, "Legs", ["Calves"]),
        new("Mobility Flow", "Gentle movement through various ranges of motion", ExerciseDifficulty.Beginner, Equipment.None, "Back", ["Core", "Shoulders"]),
        new("Static Stretching", "Hold stretched positions for extended time", ExerciseDifficulty.Beginner, Equipment.None, "Back", ["Legs"]),
        new("Dynamic Stretching", "Moving stretches through full range of motion", ExerciseDifficulty.Beginner, Equipment.None, "Back", ["Legs", "Core"]),
        new("Cat-Cow Stretch", "Spinal mobility exercise alternating between flexion and extension", ExerciseDifficulty.Beginner, Equipment.None, "Back", ["Core"]),
        new("World's Greatest Stretch", "Complex stretch hitting multiple muscle groups", ExerciseDifficulty.Novice, Equipment.None, "Back", ["Legs", "Core", "Shoulders"]),
        new("Wall Slides", "Shoulder mobility exercise against wall", ExerciseDifficulty.Beginner, Equipment.None, "Shoulders", []),
        new("Hip Flexor Stretch", "Kneeling stretch targeting hip flexors", ExerciseDifficulty.Beginner, Equipment.None, "Legs", ["Core"]),
        new("Thoracic Extensions", "Upper back mobility over foam roller", ExerciseDifficulty.Beginner, Equipment.None, "Back", []),
        new("Band Pull-Aparts", "Shoulder health exercise using resistance band", ExerciseDifficulty.Beginner, Equipment.ResistanceBand, "Shoulders", ["Back"]),

        // Flexibility
        new("Yoga Poses", "Various yoga positions for flexibility and balance", ExerciseDifficulty.Novice, Equipment.None, "Back", ["Core", "Legs"]),
        new("Jefferson Curls", "Weighted forward fold for spinal flexibility", ExerciseDifficulty.Advanced, Equipment.Barbell, "Back", ["Legs"]),
        new("Loaded Progressive Stretching", "Gradually increasing range of motion with weight", ExerciseDifficulty.Intermediate, Equipment.Dumbbell, "Back", ["Legs"]),
        new("Wall Splits", "Supported splits progression using wall", ExerciseDifficulty.Advanced, Equipment.None, "Legs", []),
        new("Bridge Pose", "Back-bending exercise for spine flexibility", ExerciseDifficulty.Intermediate, Equipment.None, "Back", ["Shoulders"]),
        new("Shoulder Dislocates", "Shoulder mobility exercise with stick or band", ExerciseDifficulty.Intermediate, Equipment.ResistanceBand, "Shoulders", ["Back"]),
        new("Pancake Stretch", "Seated forward fold with wide legs", ExerciseDifficulty.Advanced, Equipment.None, "Legs", ["Back"]),
        new("Hip 90/90", "Hip mobility exercise in 90/90 position", ExerciseDifficulty.Intermediate, Equipment.None, "Legs", ["Core"]),
        new("Ankle Mobility", "Various exercises to improve ankle range of motion", ExerciseDifficulty.Beginner, Equipment.None, "Legs", []),
        new("Wrist Mobility", "Exercises to improve wrist flexibility and strength", ExerciseDifficulty.Beginner, Equipment.None, "Forearms", [])
    };

    private static List<Category> trainingPlanCategories = new()
    {
        new("Full Body", "Complete workouts targeting all major muscle groups"),
        new("Split Routine", "Focused workouts targeting specific muscle groups"),
        new("Strength", "Programs focused on building strength and muscle"),
        new("Weight Loss", "Programs designed for fat loss and conditioning"),
        new("Functional Fitness", "Workouts improving everyday movement patterns"),
        new("Endurance", "Programs focused on building stamina and endurance"),
        new("Flexibility", "Programs for improving mobility and flexibility"),
        new("HIIT", "High-intensity interval training programs"),
        new("Upper/Lower", "Split routines focusing on upper and lower body")
    };

    private static List<Plan> defaultTrainingPlans = new()
    {
        // Full Body Workouts
        new(
            "Full Body Strength",
            "Efficient strength-focused workout targeting all major muscle groups",
            new[] { "Full Body", "Strength" },
            new[]
            {
                new Activity("Back Squat", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Flat Barbell Bench Press", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Deadlift", new[] { new Set(5), new Set(5), new Set(5) }),
                new Activity("Pull-Ups", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Overhead Press", new[] { new Set(8), new Set(8), new Set(8) }),
                new Activity("Plank", new[] { new Set(0) { RestAfterDuration = 60 }, new Set(0) { RestAfterDuration = 60 }, new Set(0) { RestAfterDuration = 60 } })
            }
        ),
        new(
            "Full Body Hypertrophy",
            "Volume-focused workout for muscle growth",
            new[] { "Full Body" },
            new[]
            {
                new Activity("Back Squat", new[] { new Set(10), new Set(10), new Set(10), new Set(10) }),
                new Activity("Incline Dumbbell Press", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Romanian Deadlift", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Barbell Rows", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Lateral Raises", new[] { new Set(15), new Set(15), new Set(15) })
            }
        ),
        new(
            "Full Body HIIT",
            "High-intensity full body workout for fat loss",
            new[] { "Full Body", "HIIT", "Weight Loss" },
            new[]
            {
                new Activity("Kettlebell Swings", new[] { new Set(20), new Set(20), new Set(20), new Set(20) }),
                new Activity("Dumbbell Thrusters", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Burpees", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Mountain Climbers", new[] { new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 } }),
                new Activity("Deadlift", new[] { new Set(8), new Set(8), new Set(8) })
            }
        ),
        
        // Push/Pull/Legs Split
        new(
            "Push Day",
            "Chest, shoulders, and triceps focused workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Flat Barbell Bench Press", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Overhead Press", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Dumbbell Flyes", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Lateral Raises", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Tricep Pushdowns", new[] { new Set(15), new Set(15), new Set(15) })
            }
        ),
        new(
            "Pull Day",
            "Back and biceps focused workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Deadlift", new[] { new Set(5), new Set(5), new Set(5), new Set(5) }),
                new Activity("Pull-Ups", new[] { new Set(10), new Set(10), new Set(10), new Set(10) }),
                new Activity("Barbell Rows", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Barbell Curl", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Face Pulls", new[] { new Set(15), new Set(15), new Set(15) })
            }
        ),
        new(
            "Legs Day",
            "Lower body focused workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Back Squat", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Romanian Deadlift", new[] { new Set(8), new Set(8), new Set(8) }),
                new Activity("Bulgarian Split Squats", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Leg Press", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Standing Calf Raises", new[] { new Set(20), new Set(20), new Set(20), new Set(20) })
            }
        ),

        // Bro Split
        new(
            "Chest Day",
            "Intensive chest workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Flat Barbell Bench Press", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Incline Dumbbell Press", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Cable Crossovers", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Push-Ups", new[] { new Set(0) }) // To failure
            }
        ),
        new(
            "Back Day",
            "Comprehensive back workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Pull-Ups", new[] { new Set(10), new Set(10), new Set(10), new Set(10) }),
                new Activity("Deadlift", new[] { new Set(5), new Set(5), new Set(5), new Set(5) }),
                new Activity("Single-Arm Dumbbell Row", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Lat Pulldown", new[] { new Set(12), new Set(12), new Set(12) })
            }
        ),
        new(
            "Shoulders Day",
            "Focused shoulder development workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Overhead Press", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Arnold Press", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Lateral Raises", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Reverse Flyes", new[] { new Set(12), new Set(12), new Set(12) })
            }
        ),
        new(
            "Arms Day",
            "Biceps and triceps focused workout",
            new[] { "Split Routine" },
            new[]
            {
                new Activity("Barbell Curl", new[] { new Set(10), new Set(10), new Set(10), new Set(10) }),
                new Activity("Hammer Curls", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Tricep Dips", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Skull Crushers", new[] { new Set(10), new Set(10), new Set(10) })
            }
        ),

        // Upper/Lower Split
        new(
            "Upper Body Strength",
            "Comprehensive upper body strength workout",
            new[] { "Upper/Lower", "Strength" },
            new[]
            {
                new Activity("Flat Barbell Bench Press", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Pull-Ups", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Overhead Press", new[] { new Set(8), new Set(8), new Set(8) }),
                new Activity("Barbell Rows", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Barbell Curl", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Tricep Dips", new[] { new Set(12), new Set(12), new Set(12) })
            }
        ),
        new(
            "Lower Body Strength",
            "Intensive lower body strength session",
            new[] { "Upper/Lower", "Strength" },
            new[]
            {
                new Activity("Back Squat", new[] { new Set(8), new Set(8), new Set(8), new Set(8) }),
                new Activity("Deadlift", new[] { new Set(5), new Set(5), new Set(5) }),
                new Activity("Leg Press", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Bulgarian Split Squats", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Standing Calf Raises", new[] { new Set(20), new Set(20), new Set(20) })
            }
        ),

        // Functional Fitness
        new(
            "Core and Stability",
            "Workout focused on core strength and stability",
            new[] { "Functional Fitness" },
            new[]
            {
                new Activity("Plank", new[] { new Set(0) { RestAfterDuration = 60 }, new Set(0) { RestAfterDuration = 60 }, new Set(0) { RestAfterDuration = 60 } }),
                new Activity("Side Plank", new[] { new Set(0) { RestAfterDuration = 45 }, new Set(0) { RestAfterDuration = 45 }, new Set(0) { RestAfterDuration = 45 } }),
                new Activity("Dead Bug", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Bird Dog", new[] { new Set(15), new Set(15), new Set(15) })
            }
        ),
        new(
            "Balance and Agility",
            "Workout for improving balance and movement control",
            new[] { "Functional Fitness" },
            new[]
            {
                new Activity("Single-Leg Deadlift", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Box Jumps", new[] { new Set(8), new Set(8), new Set(8) }),
                new Activity("Bulgarian Split Squats", new[] { new Set(10), new Set(10), new Set(10) }),
                new Activity("Farmer's Walk", new[] { new Set(0) { RestAfterDuration = 30 }, new Set(0) { RestAfterDuration = 30 }, new Set(0) { RestAfterDuration = 30 } })
            }
        ),
        new(
            "Functional Endurance",
            "Circuit training for functional endurance",
            new[] { "Functional Fitness", "Endurance" },
            new[]
            {
                new Activity("Kettlebell Swings", new[] { new Set(20), new Set(20), new Set(20) }),
                new Activity("Clean and Jerk", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Farmer's Walk", new[] { new Set(0) { RestAfterDuration = 30 }, new Set(0) { RestAfterDuration = 30 }, new Set(0) { RestAfterDuration = 30 }, new Set(0) { RestAfterDuration = 30 } })
            }
        ),

        // Weight Loss/HIIT Programs
        new(
            "HIIT Circuit A",
            "High-intensity interval training for fat loss - Circuit A",
            new[] { "Weight Loss", "HIIT" },
            new[]
            {
                new Activity("Jump Squats", new[] { new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 } }),
                new Activity("Push-Ups", new[] { new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 } }),
                new Activity("Burpees", new[] { new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 } }),
                new Activity("Mountain Climbers", new[] { new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 } }),
                new Activity("High Knees", new[] { new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 }, new Set(0) { RestAfterDuration = 40 } })
            }
        ),
        new(
            "HIIT Circuit B",
            "High-intensity interval training for fat loss - Circuit B",
            new[] { "Weight Loss", "HIIT" },
            new[]
            {
                new Activity("Kettlebell Swings", new[] { new Set(20), new Set(20), new Set(20) }),
                new Activity("Dumbbell Thrusters", new[] { new Set(12), new Set(12), new Set(12) }),
                new Activity("Box Jumps", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Russian Twists", new[] { new Set(20), new Set(20), new Set(20) })
            }
        ),

        // Endurance Programs
        new(
            "Endurance Builder",
            "Progressive endurance training program",
            new[] { "Endurance" },
            new[]
            {
                new Activity("Pull-Ups", new[] { new Set(12), new Set(12), new Set(12), new Set(12) }),
                new Activity("Push-Ups", new[] { new Set(20), new Set(20), new Set(20), new Set(20) }),
                new Activity("Back Squat", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Rowing", new[] { new Set(0) { RestAfterDuration = 300 } }) // 5 minutes
            }
        ),
        new(
            "Cardio Conditioning",
            "Mixed cardio workout for improved stamina",
            new[] { "Endurance", "Weight Loss" },
            new[]
            {
                new Activity("High Knees", new[] { new Set(0) { RestAfterDuration = 60 }, new Set(0) { RestAfterDuration = 60 } }),
                new Activity("Mountain Climbers", new[] { new Set(0) { RestAfterDuration = 60 }, new Set(0) { RestAfterDuration = 60 } }),
                new Activity("Burpees", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Jump Rope", new[] { new Set(0) { RestAfterDuration = 120 }, new Set(0) { RestAfterDuration = 120 } }) // 2-minute rounds
            }
        ),

        // Flexibility and Mobility Programs
        new(
            "Mobility Flow",
            "Dynamic stretching routine for improved flexibility",
            new[] { "Flexibility" },
            new[]
            {
                new Activity("Cat-Cow Stretch", new[] { new Set(0) { RestAfterDuration = 120 } }),
                new Activity("World's Greatest Stretch", new[] { new Set(0) { RestAfterDuration = 120 } }),
                new Activity("Wall Slides", new[] { new Set(15), new Set(15), new Set(15) }),
                new Activity("Hip Flexor Stretch", new[] { new Set(0) { RestAfterDuration = 60 } })
            }
        ),
        new(
            "Yoga Flow",
            "Yoga-inspired flexibility and mindfulness routine",
            new[] { "Flexibility" },
            new[]
            {
                new Activity("Cat-Cow Stretch", new[] { new Set(0) { RestAfterDuration = 120 } }), // 2 minutes
                new Activity("Downward Dog", new[] { new Set(0) { RestAfterDuration = 120 } }), // 2 minutes
                new Activity("Hip Flexor Stretch", new[] { new Set(0) { RestAfterDuration = 120 } }), // 2 minutes per side
                new Activity("World's Greatest Stretch", new[] { new Set(0) { RestAfterDuration = 120 } }), // 2 minutes per side
                new Activity("Wall Slides", new[] { new Set(0) { RestAfterDuration = 120 } }) // 2 minutes
            }
        ),

        // Strength-Focused Programs
        new(
            "Pure Strength Upper",
            "Heavy upper body strength focus",
            new[] { "Strength", "Upper/Lower" },
            new[]
            {
                new Activity("Flat Barbell Bench Press", new[] { new Set(6), new Set(6), new Set(6), new Set(6) }),
                new Activity("Overhead Press", new[] { new Set(6), new Set(6), new Set(6) }),
                new Activity("Pull-Ups", new[] { new Set(8), new Set(8), new Set(8) }),
                new Activity("Barbell Rows", new[] { new Set(6), new Set(6), new Set(6) })
            }
        ),
        new(
            "Pure Strength Lower",
            "Heavy lower body strength focus",
            new[] { "Strength", "Upper/Lower" },
            new[]
            {
                new Activity("Back Squat", new[] { new Set(5), new Set(5), new Set(5), new Set(5) }),
                new Activity("Deadlift", new[] { new Set(5), new Set(5), new Set(5) }),
                new Activity("Romanian Deadlift", new[] { new Set(8), new Set(8), new Set(8) }),
                new Activity("Bulgarian Split Squats", new[] { new Set(8), new Set(8), new Set(8) })
            }
        ),
        new(
            "Olympic Lifting",
            "Technical Olympic weightlifting session",
            new[] { "Strength" },
            new[]
            {
                new Activity("Clean and Jerk", new[] { new Set(6), new Set(6), new Set(6) }),
                new Activity("Power Clean", new[] { new Set(5), new Set(5), new Set(5) }),
                new Activity("Front Squat", new[] { new Set(6), new Set(6), new Set(6) }),
                new Activity("Push Press", new[] { new Set(8), new Set(8), new Set(8) })
            }
        )
    };
}