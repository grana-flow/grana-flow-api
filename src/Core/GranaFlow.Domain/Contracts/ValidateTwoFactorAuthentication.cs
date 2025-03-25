using System.ComponentModel.DataAnnotations;

namespace GranaFlow.Domain.Contracts;

public record class ValidateTwoFactorAuthentication
{
    [Required(ErrorMessage = "{0} is required")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    public required string Token { get; set; }
}
