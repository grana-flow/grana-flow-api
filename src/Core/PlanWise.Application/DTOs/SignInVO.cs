using System.ComponentModel.DataAnnotations;

namespace PlanWise.Application.DTOs;

public class SignInVO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}
