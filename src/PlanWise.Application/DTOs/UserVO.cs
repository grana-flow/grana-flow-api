using System.ComponentModel.DataAnnotations;

namespace PlanWise.Application.DTOs;

public class UserVO
{
    [Required(ErrorMessage = "{0} is required")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public required string ConfirmPassword { get; set; }
}
