using System.ComponentModel.DataAnnotations;

namespace PlanWise.Domain.Contracts;

public record class ResetPassword
{
    [Required(ErrorMessage = "{0} is required")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public required string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    public required string Email { get; set; }
}
