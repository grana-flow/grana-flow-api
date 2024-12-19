﻿using System.ComponentModel.DataAnnotations;

namespace PlanWise.Application.DTOs;

public class ResetPasswordVO
{
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public required string ConfirmPassword { get; set; }

    [Required]
    public required string Email { get; set; }
}
