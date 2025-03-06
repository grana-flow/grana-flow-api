using System.ComponentModel.DataAnnotations;

namespace PlanWise.Domain.Contracts;

public record class CreateUser
{
    [Required(ErrorMessage = "{0} is required")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public required string ConfirmPassword { get; set; }
}
