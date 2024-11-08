using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GymTracker.Data.Validation;
using Microsoft.AspNetCore.Identity;

namespace GymTracker.Data;

public class ApplicationUser : IdentityUser
{
    [Required]
    [PersonalData]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 20 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens and apostrophes")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [PersonalData]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 30 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens and apostrophes")]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    [Required]
    [PersonalData]
    public Gender Gender { get; set; } = Gender.Male;

    [Required]
    [PersonalData]
    [DataType(DataType.Date)]
    [MinimumBirthDate]
    public DateOnly BirthDate { get; set; }

    [DataType(DataType.Date)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateOnly JoinDate { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [NotMapped]
    public int Age => CalculateAge();

    private int CalculateAge()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - BirthDate.Year;
        if (BirthDate > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }
}
