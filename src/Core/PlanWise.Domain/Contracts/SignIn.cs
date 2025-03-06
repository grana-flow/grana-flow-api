using System.ComponentModel.DataAnnotations;

namespace PlanWise.Domain.Contracts;

public record class SignIn
{
    [Required(ErrorMessage = "{0} is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    public required string Password { get; set; }
}
