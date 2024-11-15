namespace GymTracker.Data;

public class TrainingPlanCategory : CategoryBase
{
    public virtual List<DefaultTrainingPlan> DefaultTrainingPlans { get; set; } = [];
    public virtual List<UserMadeTrainingPlan> UserMadeTrainingPlans { get; set; } = [];
}
