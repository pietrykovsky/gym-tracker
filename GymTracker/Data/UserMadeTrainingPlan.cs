using System;
using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class UserMadeTrainingPlan : TrainingPlanBase
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;

    public virtual List<TrainingPlanCategory> Categories { get; set; } = [];
}
