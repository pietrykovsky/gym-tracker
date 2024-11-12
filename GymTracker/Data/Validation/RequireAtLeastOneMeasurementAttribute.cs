using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data.Validation;

[AttributeUsage(AttributeTargets.Class)]
public class RequireAtLeastOneMeasurementAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not BodyMeasurement measurement)
        {
            return new ValidationResult("Invalid object type");
        }

        // Get all nullable float properties except Date and Notes
        var measurementProperties = typeof(BodyMeasurement)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(float?) &&
                       p.Name != nameof(BodyMeasurement.BMI)); // Exclude BMI as it's calculated

        // Check if at least one property has a value
        bool hasValue = measurementProperties.Any(prop =>
            prop.GetValue(measurement) is float value && value > 0);

        if (!hasValue)
        {
            return new ValidationResult("At least one measurement value must be provided.");
        }

        return ValidationResult.Success;
    }
}