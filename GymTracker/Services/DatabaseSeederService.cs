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
        foreach (var category in exerciseCategories)
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

                await context.TrainingActivities.AddRangeAsync(activities);
            }
        }

        await context.SaveChangesAsync();
    }

    private record Category(string Name, string Description);
    private record Exercise(string Name, string Description, ExerciseDifficulty Difficulty, IEnumerable<string> Categories);
    private record Set(int Repetitions, float? Weight = null, int? RestAfterDuration = 60);
    private record Activity(string ExerciseName, IEnumerable<Set> Sets);
    private record Plan(string Name, string Description, IEnumerable<string> Categories, IEnumerable<Activity> Activities);

    private static List<Category> exerciseCategories = new()
    {
        new ("Chest", "Exercises targeting chest muscles including pectoralis major and minor"),
        new ("Back", "Exercises for back development including latissimus dorsi, trapezius, and rhomboids"),
        new ("Legs", "Lower body exercises targeting quadriceps, hamstrings, glutes, and calves"),
        new ("Shoulders", "Exercises for all three deltoid heads and rotator cuff muscles"),
        new ("Arms", "Isolation and compound exercises for biceps, triceps, and forearms"),
        new ("Core", "Exercises for abdominals, obliques, and lower back muscles"),
        new ("Cardio", "Aerobic exercises for cardiovascular health and endurance"),
        new ("Flexibility", "Stretching and mobility work for improved range of motion"),
        new ("Olympic", "Technical Olympic weightlifting movements and variations"),
        new ("Plyometrics", "Explosive movements for power and athletic performance"),
        new ("Functional", "Exercises mimicking everyday movement patterns"),
        new ("Calisthenics", "Bodyweight exercises for strength and control"),
        new ("Recovery", "Low-intensity exercises and techniques for active recovery")
    };

    private static List<Exercise> defaultExercises = new()
    {
        // Chest exercises
        new("Flat Barbell Bench Press", "Lie on a flat bench, grip barbell slightly wider than shoulder width, lower to chest and press back up while maintaining proper back arch and foot position", ExerciseDifficulty.Intermediate, ["Chest"]),
        new("Incline Dumbbell Press", "On an incline bench (30-45 degrees), press dumbbells from shoulder level to full extension, targeting upper chest", ExerciseDifficulty.Intermediate, ["Chest"]),
        new("Decline Bench Press", "Perform bench press on a decline bench to target lower chest fibers", ExerciseDifficulty.Intermediate, ["Chest"]),
        new("Dumbbell Flyes", "Lie flat holding dumbbells above chest, lower weights in wide arc maintaining slight elbow bend, focus on chest stretch", ExerciseDifficulty.Intermediate, ["Chest"]),
        new("Push-Ups", "Standard push-up position, hands slightly wider than shoulders, lower body until chest nearly touches ground", ExerciseDifficulty.Beginner, ["Chest", "Core"]),
        new("Cable Crossovers", "Stand between cable machines, perform crossing motion with arms from high to low position", ExerciseDifficulty.Novice, ["Chest"]),
        new("Chest Dips", "Support body on parallel bars, lean forward slightly, lower body by bending elbows until chest stretch is felt, then press back up", ExerciseDifficulty.Intermediate, ["Chest", "Arms"]),
        new("Machine Chest Press", "Sit at chest press machine, grip handles at chest level, press forward until arms are extended", ExerciseDifficulty.Beginner, ["Chest"]),
        new("Landmine Press", "Hold barbell end at shoulder, press up and forward in an arc motion", ExerciseDifficulty.Novice, ["Chest", "Shoulders"]),
        new("Smith Machine Bench Press", "Perform bench press using Smith machine for guided movement pattern", ExerciseDifficulty.Novice, ["Chest"]),
        new("Incline Barbell Bench Press", "Bench press performed on an incline bench to target upper chest", ExerciseDifficulty.Intermediate, ["Chest"]),
        new("Decline Dumbbell Press", "Press dumbbells while lying on a decline bench", ExerciseDifficulty.Intermediate, ["Chest"]),

        // Back exercises
        new("Deadlift", "Stand with feet hip-width, bend at hips and knees to grip barbell, maintain neutral spine while lifting bar to hip level", ExerciseDifficulty.Advanced, ["Back", "Legs"]),
        new("Pull-Ups", "Hang from bar with wide grip, pull body up until chin clears bar, focus on engaging lats", ExerciseDifficulty.Intermediate, ["Back", "Arms"]),
        new("Barbell Rows", "Bend forward at hips holding barbell, pull weight to lower chest while keeping back straight", ExerciseDifficulty.Intermediate, ["Back"]),
        new("Seated Cable Rows", "Sit at cable machine, pull handle to torso while maintaining upright posture", ExerciseDifficulty.Beginner, ["Back"]),
        new("Face Pulls", "Use rope attachment on cable machine at head height, pull towards face while separating rope ends", ExerciseDifficulty.Novice, ["Back", "Shoulders"]),
        new("Single-Arm Dumbbell Row", "One knee and hand on bench, row dumbbell to hip while keeping back straight", ExerciseDifficulty.Novice, ["Back"]),
        new("Meadows Row", "Perform single-arm rows with barbell from landmine attachment", ExerciseDifficulty.Intermediate, ["Back"]),
        new("Straight-Arm Pulldown", "Use cable machine, keep arms straight while pulling bar down to thighs", ExerciseDifficulty.Novice, ["Back", "Chest"]),
        new("T-Bar Row", "Straddle T-bar machine or barbell, pull weight up towards chest", ExerciseDifficulty.Intermediate, ["Back"]),
        new("Rack Pulls", "Partial deadlift movement from elevated starting position", ExerciseDifficulty.Advanced, ["Back", "Legs"]),
        new("Chin-Ups", "Pull-ups with underhand grip, targeting biceps more", ExerciseDifficulty.Intermediate, ["Back", "Arms"]),
        new("Neutral Grip Pull-Ups", "Pull-ups using parallel grip handles", ExerciseDifficulty.Intermediate, ["Back", "Arms"]),
        new("Pendlay Row", "Barbell row starting each rep from the floor", ExerciseDifficulty.Advanced, ["Back"]),
        new("Lat Pulldown", "Cable pulldown exercise targeting latissimus dorsi", ExerciseDifficulty.Beginner, ["Back"]),

        // Legs exercises
        new("Back Squat", "Barbell across upper back, feet shoulder-width, squat down keeping chest up and knees tracking over toes", ExerciseDifficulty.Advanced, ["Legs"]),
        new("Front Squat", "Barbell in front rack position, perform squat maintaining upright torso", ExerciseDifficulty.Advanced, ["Legs", "Core"]),
        new("Romanian Deadlift", "Hold barbell at hips, hinge forward maintaining slight knee bend and flat back", ExerciseDifficulty.Intermediate, ["Legs", "Back"]),
        new("Walking Lunges", "Step forward into lunge position, lower back knee toward ground, alternate legs", ExerciseDifficulty.Novice, ["Legs"]),
        new("Bulgarian Split Squats", "Rear foot elevated on bench, perform single-leg squat with front leg", ExerciseDifficulty.Intermediate, ["Legs"]),
        new("Leg Press", "Sit in machine with feet shoulder-width, press weight while maintaining lower back contact", ExerciseDifficulty.Beginner, ["Legs"]),
        new("Standing Calf Raises", "Stand on edge of platform, raise heels up and lower back down slowly", ExerciseDifficulty.Beginner, ["Legs"]),
        new("Seated Calf Raises", "Perform calf raises while seated with weight on knees", ExerciseDifficulty.Beginner, ["Legs"]),
        new("Hack Squat", "Use hack squat machine, position shoulders and back against pad, squat down", ExerciseDifficulty.Intermediate, ["Legs"]),
        new("Good Mornings", "Barbell on upper back, hinge at hips while maintaining slight knee bend", ExerciseDifficulty.Intermediate, ["Legs", "Back"]),
        new("Leg Extensions", "Sit in machine, extend legs to straight position, focusing on quad contraction", ExerciseDifficulty.Beginner, ["Legs"]),
        new("Lying Leg Curls", "Lie face down on machine, curl weight using hamstrings", ExerciseDifficulty.Beginner, ["Legs"]),
        new("Sissy Squats", "Hold support, lean back and bend knees while keeping hips straight", ExerciseDifficulty.Advanced, ["Legs"]),
        new("Box Squats", "Perform squat to box, pause, then drive up", ExerciseDifficulty.Intermediate, ["Legs"]),
        new("Step-Ups", "Step up onto elevated platform, driving through heel", ExerciseDifficulty.Novice, ["Legs"]),
        new("Glute Bridge", "Lie on back, drive hips up while squeezing glutes", ExerciseDifficulty.Beginner, ["Legs"]),
        new("Hip Thrust", "Rest upper back on bench, drive hips up with weight on lap", ExerciseDifficulty.Novice, ["Legs"]),

        // Shoulders exercises
        new("Overhead Press", "Stand holding barbell at shoulders, press overhead while maintaining stable core", ExerciseDifficulty.Advanced, ["Shoulders", "Core"]),
        new("Lateral Raises", "Stand holding dumbbells at sides, raise arms out to shoulder level maintaining slight bend", ExerciseDifficulty.Beginner, ["Shoulders"]),
        new("Front Raises", "Hold weights in front of thighs, raise to shoulder height keeping arms straight", ExerciseDifficulty.Beginner, ["Shoulders"]),
        new("Reverse Flyes", "Bend forward holding dumbbells, raise arms out to sides focusing on rear deltoids", ExerciseDifficulty.Novice, ["Shoulders"]),
        new("Arnold Press", "Seated press starting with palms facing you, rotate to palms forward while pressing up", ExerciseDifficulty.Intermediate, ["Shoulders"]),
        new("Military Press", "Strict overhead press with feet together, maintaining rigid body position", ExerciseDifficulty.Advanced, ["Shoulders", "Core"]),
        new("Behind the Neck Press", "Press barbell from behind neck position to overhead", ExerciseDifficulty.Advanced, ["Shoulders"]),
        new("Cable Lateral Raises", "Perform lateral raises using low cable pulley", ExerciseDifficulty.Novice, ["Shoulders"]),
        new("Upright Rows", "Pull barbell or dumbbells vertically along body to chin height", ExerciseDifficulty.Intermediate, ["Shoulders"]),
        new("Plate Front Raises", "Hold weight plate with arms extended, raise to shoulder height", ExerciseDifficulty.Novice, ["Shoulders"]),
        new("Face Pull with External Rotation", "Face pull variation emphasizing rotator cuff engagement", ExerciseDifficulty.Novice, ["Shoulders"]),
        new("Landmine Lateral Raises", "One-arm lateral raise using landmine attachment", ExerciseDifficulty.Novice, ["Shoulders"]),

        // Arms exercises
        new("Barbell Curl", "Stand holding barbell with underhand grip, curl weight while keeping upper arms static", ExerciseDifficulty.Novice, ["Arms"]),
        new("Skull Crushers", "Lie on bench holding weight above forehead, lower behind head by bending elbows", ExerciseDifficulty.Novice, ["Arms"]),
        new("Hammer Curls", "Perform bicep curls with neutral grip (palms facing each other)", ExerciseDifficulty.Beginner, ["Arms"]),
        new("Tricep Pushdowns", "Use cable machine with straight or rope attachment, extend arms downward", ExerciseDifficulty.Beginner, ["Arms"]),
        new("Preacher Curls", "Perform bicep curls with arms supported on preacher bench", ExerciseDifficulty.Novice, ["Arms"]),
        new("Diamond Push-Ups", "Push-ups with hands close together forming diamond shape, targeting triceps", ExerciseDifficulty.Intermediate, ["Arms", "Chest"]),
        new("Tricep Dips", "Support body on parallel bars, keep torso vertical, bend and extend arms", ExerciseDifficulty.Intermediate, ["Arms"]),
        new("Concentration Curls", "Seated single-arm curl with elbow braced against inner thigh", ExerciseDifficulty.Beginner, ["Arms"]),
        new("Overhead Tricep Extension", "Hold weight overhead, lower behind head by bending elbows", ExerciseDifficulty.Novice, ["Arms"]),
        new("21s", "Perform bicep curls in three ranges of motion, 7 reps each", ExerciseDifficulty.Intermediate, ["Arms"]),
        new("Reverse Curls", "Curl weight using overhand grip to target forearms", ExerciseDifficulty.Novice, ["Arms"]),
        new("Close-Grip Bench Press", "Perform bench press with narrow hand spacing for triceps", ExerciseDifficulty.Intermediate, ["Arms", "Chest"]),
        new("Spider Curls", "Perform curls lying face down on incline bench", ExerciseDifficulty.Novice, ["Arms"]),
        new("Zottman Curls", "Curl up with supinated grip, rotate to pronated grip for lowering", ExerciseDifficulty.Intermediate, ["Arms"]),
        new("Cable Hammer Curls", "Perform hammer curls using cable machine", ExerciseDifficulty.Beginner, ["Arms"]),
        new("JM Press", "Hybrid of close-grip bench press and skull crusher", ExerciseDifficulty.Advanced, ["Arms", "Chest"]),

        // Core exercises
        new("Plank", "Hold push-up position on forearms, maintaining straight body alignment", ExerciseDifficulty.Beginner, ["Core"]),
        new("Russian Twists", "Seated with feet off ground, rotate torso side to side holding weight", ExerciseDifficulty.Novice, ["Core"]),
        new("Cable Woodchops", "Stand sideways to cable machine, rotate torso pulling weight across body", ExerciseDifficulty.Novice, ["Core"]),
        new("Hanging Leg Raises", "Hang from pull-up bar, raise legs to parallel while keeping them straight", ExerciseDifficulty.Advanced, ["Core"]),
        new("Ab Wheel Rollouts", "Kneel holding ab wheel, roll forward and back maintaining core tension", ExerciseDifficulty.Advanced, ["Core"]),
        new("Dragon Flag", "Advanced movement keeping body straight while rolling up and down bench", ExerciseDifficulty.Expert, ["Core"]),
        new("Cable Crunches", "Kneel at cable machine, crunch down pulling rope attachment", ExerciseDifficulty.Novice, ["Core"]),
        new("Windshield Wipers", "Hang from pull-up bar, swing legs side to side while keeping straight", ExerciseDifficulty.Expert, ["Core"]),
        new("Copenhagen Plank", "Side plank with top leg supported, bottom leg raised", ExerciseDifficulty.Advanced, ["Core"]),
        new("Pallof Press", "Anti-rotation exercise using cable machine", ExerciseDifficulty.Intermediate, ["Core"]),
        new("Turkish Get-Up", "Complex movement from lying to standing while holding weight overhead", ExerciseDifficulty.Advanced, ["Core", "Shoulders"]),
        new("Dead Bug", "Lie on back, alternately extend opposite arm and leg", ExerciseDifficulty.Beginner, ["Core"]),
        new("Bird Dog", "Quadruped position, extend opposite arm and leg", ExerciseDifficulty.Beginner, ["Core"]),
        new("Side Plank", "Balance on forearm and side of feet, maintain straight body", ExerciseDifficulty.Novice, ["Core"]),

        // Cardio exercises
        new("High-Intensity Interval Training", "Alternate between intense exercise and rest periods", ExerciseDifficulty.Intermediate, ["Cardio"]),
        new("Rowing", "Use rowing machine maintaining proper form through drive and recovery phases", ExerciseDifficulty.Novice, ["Cardio"]),
        new("Jump Rope", "Basic bouncing or advanced variations for cardio development", ExerciseDifficulty.Novice, ["Cardio"]),
        new("Stair Climber", "Use stair climbing machine maintaining upright posture", ExerciseDifficulty.Beginner, ["Cardio"]),
        new("Sprint Intervals", "Alternating between sprinting and jogging/walking", ExerciseDifficulty.Intermediate, ["Cardio"]),
        new("Cycling", "Indoor or outdoor cycling for cardiovascular endurance", ExerciseDifficulty.Beginner, ["Cardio"]),
    // Cardio continued
        new("Mountain Climbers", "Plank position, alternately drive knees towards chest", ExerciseDifficulty.Novice, ["Cardio", "Core"]),
        new("Battle Ropes", "Create waves or slams with heavy ropes", ExerciseDifficulty.Intermediate, ["Cardio", "Arms"]),

        // Plyometrics
        new("Box Jumps", "Explosive jump onto raised platform, step back down", ExerciseDifficulty.Intermediate, ["Plyometrics", "Legs"]),
        new("Jump Squats", "Perform squat with explosive jump at top of movement", ExerciseDifficulty.Intermediate, ["Plyometrics", "Legs"]),
        new("Depth Jumps", "Step off box, land and immediately jump vertically", ExerciseDifficulty.Advanced, ["Plyometrics", "Legs"]),
        new("Plyo Push-Ups", "Explosive push-ups with hands leaving ground", ExerciseDifficulty.Advanced, ["Plyometrics", "Chest", "Arms"]),
        new("Broad Jumps", "Horizontal jumping for distance", ExerciseDifficulty.Intermediate, ["Plyometrics", "Legs"]),
        new("Bounding", "Exaggerated running steps for distance", ExerciseDifficulty.Intermediate, ["Plyometrics", "Legs"]),
        new("Medicine Ball Slams", "Explosively slam medicine ball to ground", ExerciseDifficulty.Novice, ["Plyometrics", "Core"]),

        // Olympic
        new("Clean and Jerk", "Two-part Olympic lift: explosive pull to shoulders, then drive overhead", ExerciseDifficulty.Expert, ["Olympic", "Legs", "Shoulders"]),
        new("Snatch", "Single explosive movement from ground to overhead with wide grip", ExerciseDifficulty.Expert, ["Olympic", "Legs", "Shoulders"]),
        new("Power Clean", "Clean variation caught in partial squat position", ExerciseDifficulty.Advanced, ["Olympic", "Legs"]),
        new("Hang Clean", "Clean starting from hanging position at thighs", ExerciseDifficulty.Advanced, ["Olympic", "Legs"]),
        new("Clean Pull", "Clean movement without catching the bar", ExerciseDifficulty.Advanced, ["Olympic", "Back", "Legs"]),
        new("Snatch Pull", "Snatch movement without catching the bar", ExerciseDifficulty.Advanced, ["Olympic", "Back", "Legs"]),
        new("Power Snatch", "Snatch caught in partial squat position", ExerciseDifficulty.Expert, ["Olympic", "Legs", "Shoulders"]),
        new("Hang Snatch", "Snatch starting from hanging position", ExerciseDifficulty.Expert, ["Olympic", "Legs", "Shoulders"]),
        new("Split Jerk", "Drive weight overhead with split leg position", ExerciseDifficulty.Advanced, ["Olympic", "Legs", "Shoulders"]),
        new("Push Press", "Overhead press with leg drive assistance", ExerciseDifficulty.Intermediate, ["Olympic", "Shoulders", "Legs"]),
        new("Muscle Snatch", "Snatch performed without dropping under bar", ExerciseDifficulty.Expert, ["Olympic", "Shoulders", "Back"]),

        // Functional
        new("Farmer's Walks", "Carry heavy dumbbells or kettlebells while walking", ExerciseDifficulty.Novice, ["Functional", "Core", "Arms"]),
        new("Sled Push/Pull", "Push or pull weighted sled across floor", ExerciseDifficulty.Intermediate, ["Functional", "Legs"]),
        new("Sandbag Carry", "Carry heavy sandbag in various positions", ExerciseDifficulty.Intermediate, ["Functional", "Core"]),
        new("Tire Flips", "Flip large tire using hip hinge and explosive movement", ExerciseDifficulty.Advanced, ["Functional", "Legs", "Back"]),
        new("Prowler Push", "Push weighted sled focusing on leg drive", ExerciseDifficulty.Intermediate, ["Functional", "Legs"]),
        new("Racked Carries", "Carry kettlebells/dumbbells in front rack position", ExerciseDifficulty.Intermediate, ["Functional", "Core", "Shoulders"]),
        new("Overhead Carries", "Walk while holding weight overhead", ExerciseDifficulty.Advanced, ["Functional", "Shoulders", "Core"]),
        new("Zercher Carry", "Carry barbell in crook of elbows", ExerciseDifficulty.Advanced, ["Functional", "Core", "Arms"]),
        new("Yoke Walk", "Walk with heavy yoke across shoulders", ExerciseDifficulty.Expert, ["Functional", "Legs", "Core"]),

        // Calisthenics
        new("Muscle-Ups", "Advanced pull-up transitioning to dip position above bar", ExerciseDifficulty.Expert, ["Calisthenics", "Back", "Arms"]),
        new("Handstand Push-Ups", "Inverted push-ups against wall for shoulder strength", ExerciseDifficulty.Expert, ["Calisthenics", "Shoulders", "Core"]),
        new("L-Sits", "Hold body above ground with legs extended horizontally", ExerciseDifficulty.Advanced, ["Calisthenics", "Core", "Arms"]),
        new("Front Lever", "Hold body parallel to ground while hanging", ExerciseDifficulty.Expert, ["Calisthenics", "Back", "Core"]),
        new("Back Lever", "Hold body parallel to ground facing upward", ExerciseDifficulty.Expert, ["Calisthenics", "Back", "Core"]),
        new("Planche", "Hold body parallel to ground on hands only", ExerciseDifficulty.Expert, ["Calisthenics", "Shoulders", "Core"]),
        new("Human Flag", "Hold body horizontally while gripping vertical pole", ExerciseDifficulty.Expert, ["Calisthenics", "Core", "Shoulders"]),
        new("Pistol Squats", "Single-leg squats with other leg extended", ExerciseDifficulty.Advanced, ["Calisthenics", "Legs"]),
        new("Ring Muscle-Ups", "Muscle-ups performed on gymnastic rings", ExerciseDifficulty.Expert, ["Calisthenics", "Back", "Arms"]),
        new("Archer Pull-Ups", "Pull-ups with one arm more extended than other", ExerciseDifficulty.Advanced, ["Calisthenics", "Back", "Arms"]),

        // Recovery
        new("Foam Rolling", "Self-myofascial release using foam roller on tight muscles", ExerciseDifficulty.Beginner, ["Recovery"]),
        new("Light Jogging", "Easy-paced jogging for active recovery", ExerciseDifficulty.Beginner, ["Recovery", "Cardio"]),
        new("Mobility Flow", "Gentle movement through various ranges of motion", ExerciseDifficulty.Beginner, ["Recovery", "Flexibility"]),
        new("Static Stretching", "Hold stretched positions for extended time", ExerciseDifficulty.Beginner, ["Recovery", "Flexibility"]),
        new("Dynamic Stretching", "Moving stretches through full range of motion", ExerciseDifficulty.Beginner, ["Recovery", "Flexibility"]),
        new("Cat-Cow Stretch", "Spinal mobility exercise alternating between flexion and extension", ExerciseDifficulty.Beginner, ["Recovery", "Flexibility"]),
        new("World's Greatest Stretch", "Complex stretch hitting multiple muscle groups", ExerciseDifficulty.Novice, ["Recovery", "Flexibility"]),
        new("Wall Slides", "Shoulder mobility exercise against wall", ExerciseDifficulty.Beginner, ["Recovery", "Shoulders"]),
        new("Hip Flexor Stretch", "Kneeling stretch targeting hip flexors", ExerciseDifficulty.Beginner, ["Recovery", "Flexibility"]),
        new("Thoracic Extensions", "Upper back mobility over foam roller", ExerciseDifficulty.Beginner, ["Recovery", "Back"]),
        new("Band Pull-Aparts", "Shoulder health exercise using resistance band", ExerciseDifficulty.Beginner, ["Recovery", "Shoulders"]),

        // Flexibility
        new("Yoga Poses", "Various yoga positions for flexibility and balance", ExerciseDifficulty.Novice, ["Flexibility", "Core"]),
        new("Jefferson Curls", "Weighted forward fold for spinal flexibility", ExerciseDifficulty.Advanced, ["Flexibility", "Back"]),
        new("Loaded Progressive Stretching", "Gradually increasing range of motion with weight", ExerciseDifficulty.Intermediate, ["Flexibility"]),
        new("Wall Splits", "Supported splits progression using wall", ExerciseDifficulty.Advanced, ["Flexibility", "Legs"]),
        new("Bridge Pose", "Back-bending exercise for spine flexibility", ExerciseDifficulty.Intermediate, ["Flexibility", "Back"]),
        new("Shoulder Dislocates", "Shoulder mobility exercise with stick or band", ExerciseDifficulty.Intermediate, ["Flexibility", "Shoulders"]),
        new("Pancake Stretch", "Seated forward fold with wide legs", ExerciseDifficulty.Advanced, ["Flexibility", "Legs"]),
        new("Hip 90/90", "Hip mobility exercise in 90/90 position", ExerciseDifficulty.Intermediate, ["Flexibility", "Legs"]),
        new("Ankle Mobility", "Various exercises to improve ankle range of motion", ExerciseDifficulty.Beginner, ["Flexibility", "Legs"]),
        new("Wrist Mobility", "Exercises to improve wrist flexibility and strength", ExerciseDifficulty.Beginner, ["Flexibility", "Arms"])
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