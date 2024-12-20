using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Data;

[Index(nameof(Name), IsUnique = true)]
public abstract class CategoryBase
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 30 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Description must be less than 200 characters.")]
    public string? Description { get; set; } = string.Empty;
}
