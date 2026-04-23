using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public class EventItem : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(100, ErrorMessage = "Event name must be 100 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(120, ErrorMessage = "Location must be 120 characters or fewer.")]
    public string Location { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Date.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "Event date cannot be in the past.",
                new[] { nameof(Date) });
        }
    }
}
