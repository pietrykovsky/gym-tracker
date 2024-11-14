using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class PlanActivity : ActivityBase
{
    [Required]
    public int PlanId { get; set; }
    public TrainingPlanBase Plan { get; set; } = null!;
}
