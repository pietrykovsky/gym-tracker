using GymTracker.Data;

namespace GymTracker.Services;

public interface IPlanGeneratorService
{
    Task<UserMadeTrainingPlan> GenerateTrainingPlanAsync(
        string userId,
        TrainingGoal goal,
        ExperienceLevel experience,
        int trainingDays,
        IEnumerable<Equipment> availableEquipment,
        PushPullWorkoutDay? pushPullDay = null,
        UpperLowerWorkoutDay? upperLowerDay = null);

    WorkoutType GetWorkoutType(ExperienceLevel experience, int trainingDays);

    void UpdatePlanWithWeights(
        UserMadeTrainingPlan plan,
        Dictionary<int, float> repMaxes,
        TrainingGoal goal,
        ExperienceLevel experience);
}
