using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GymTracker.Data.Validation;
using Microsoft.AspNetCore.Identity;

namespace GymTracker.Data;

public class ApplicationUser : IdentityUser
{
    [Required]
    [PersonalData]
    [StringLength(20)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [PersonalData]
    [StringLength(30)]
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
