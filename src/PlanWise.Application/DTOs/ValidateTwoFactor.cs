using System.ComponentModel.DataAnnotations;

namespace PlanWise.Application.DTOs;

public class ValidateTwoFactor
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Token { get; set; }
}
