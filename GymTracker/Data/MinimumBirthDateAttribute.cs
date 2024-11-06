using System.ComponentModel.DataAnnotations;

namespace GymTracker.Data.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MinimumBirthDateAttribute : ValidationAttribute
{
    private static readonly DateOnly MinDate = new(1900, 1, 1);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime)
        {
            var date = DateOnly.FromDateTime(dateTime);
            if (date < MinDate)
            {
                return new ValidationResult($"Birth date cannot be earlier than {MinDate:d}");
            }
            if (date > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return new ValidationResult("Birth date cannot be in the future");
            }
            return ValidationResult.Success;
        }
        return new ValidationResult("Invalid birth date");
    }
}
