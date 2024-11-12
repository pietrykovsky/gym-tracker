using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GymTracker.Data.Validation;

namespace GymTracker.Data;

[RequireAtLeastOneMeasurement]
public class BodyMeasurement
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly Date { get; set; }

    [Range(20, 500)]  // in kg
    [Precision(2, 1)]
    public float? Weight { get; set; }

    [Range(50, 300)]  // in cm
    [Precision(2, 1)]
    public float? Height { get; set; }

    [Range(1, 100)]
    [Precision(2, 1)]
    [Display(Name = "Fat Mass (%)")]
    public float? FatMassPercentage { get; set; }

    [Range(1, 100)]
    [Precision(2, 1)]
    [Display(Name = "Muscle Mass (%)")]
    public float? MuscleMassPercentage { get; set; }

    [Range(1, 300)]  // in cm
    [Precision(2, 1)]
    [Display(Name = "Waist Circumference")]
    public float? WaistCircumference { get; set; }

    [Range(1, 200)]  // in cm
    [Precision(2, 1)]
    [Display(Name = "Chest Circumference")]
    public float? ChestCircumference { get; set; }

    [Range(1, 100)]  // in cm
    [Precision(2, 1)]
    [Display(Name = "Arm Circumference")]
    public float? ArmCircumference { get; set; }

    [Range(1, 100)]  // in cm
    [Precision(2, 1)]
    [Display(Name = "Thigh Circumference")]
    public float? ThighCircumference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; } = string.Empty;

    [NotMapped]
    public float BMI => _calculateBMI();

    private float _calculateBMI()
    {
        if (Weight.HasValue && Weight.Value > 0 && Height.HasValue && Height.Value > 0)
        {
            var weight = Weight.Value;
            var height = Height.Value / 100; // convert height to meters
            return weight / (height * height);
        }
        return 0;
    }
}