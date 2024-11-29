using System.Collections.Generic;
using GymTracker.Data;
using GymTracker.Services;

namespace GymTracker.Tests.Services;

public class TestableTrainingPlanGenerator : PlanGeneratorService
{
    public TestableTrainingPlanGenerator(
        IDefaultExerciseService defaultExerciseService,
        IUserMadeExerciseService userExerciseService,
        ITrainingPlanCategoryService categoryService)
        : base(defaultExerciseService, userExerciseService, categoryService)
    {
    }

    public static int GetComplexityScore(ExerciseBase exercise) =>
        GetExerciseComplexityScore(exercise);

    public static (int sets, int reps, int rest) GetParameters(
        TrainingGoal goal, ExperienceLevel experience, int trainingDays) =>
        GetTrainingParameters(goal, experience, trainingDays);

    public static List<ExerciseBase> GetFullBodyExercises(List<ExerciseBase> availableExercises) =>
        SelectExercisesForFullBody(availableExercises);

    public static List<ExerciseBase> GetPushPullExercises(
        IEnumerable<ExerciseBase> availableExercises,
        PushPullWorkoutDay day,
        ExperienceLevel experience) =>
        SelectExercisesForWorkout(availableExercises, day, experience);

    public static List<ExerciseBase> GetUpperLowerExercises(
        IEnumerable<ExerciseBase> availableExercises,
        UpperLowerWorkoutDay day,
        ExperienceLevel experience) =>
        SelectExercisesForWorkout(availableExercises, day, experience);

    public static List<ExerciseBase> GetCompoundExercises(
        List<ExerciseBase> exercises,
        string primaryCategory,
        int count) =>
        GetPrimaryCompoundExercises(exercises, primaryCategory, count);

    public static List<ExerciseBase> GetIsolationMovements(
        List<ExerciseBase> exercises,
        string targetCategory,
        int count) =>
        GetIsolationExercises(exercises, targetCategory, count);
}