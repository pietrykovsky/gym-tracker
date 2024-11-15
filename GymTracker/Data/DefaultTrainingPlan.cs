namespace GymTracker.Data;

public class DefaultTrainingPlan : TrainingPlanBase
{
    public virtual List<TrainingPlanCategory> Categories { get; set; } = [];
}
