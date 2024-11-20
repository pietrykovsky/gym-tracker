using System;
using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data;

public class TrainingActivity : ActivityBase
{
    [Required]
    public int TrainingSessionId { get; set; }
    public TrainingSession TrainingSession { get; set; } = null!;
}
