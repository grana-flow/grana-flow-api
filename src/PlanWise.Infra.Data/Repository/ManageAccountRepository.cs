﻿using System.Web;
using Microsoft.AspNetCore.Identity;
using PlanWise.Domain.Entities;
using PlanWise.Domain.Interfaces;

namespace PlanWise.Infra.Data.Repository;

public class ManageAccountRepository : IManageAccountRepository
{
    private readonly UserManager<User> _userManager;

    public ManageAccountRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> CreateAccount(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<User> FindByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email)
            ?? throw new KeyNotFoundException("Email not found.");
    }

    public async Task<bool> IsEmailConfirmed(User user)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<bool> CheckPassword(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<string> GenerateEmailConfirmationToken(User user)
    {
        return HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));
    }

    public async Task<IdentityResult> ConfirmEmail(User user, string token)
    {
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<string> GenerateTwoFactorToken(User user, string provaider)
    {
        return await _userManager.GenerateTwoFactorTokenAsync(user, provaider);
    }

    public async Task<string> GenerateForgetPasswordToken(User user)
    {
        return HttpUtility.UrlEncode(await _userManager.GeneratePasswordResetTokenAsync(user));
    }

    public async Task<bool> VerifyTwoFactorToken(User user, string provaider, string token)
    {
        return await _userManager.VerifyTwoFactorTokenAsync(user, provaider, token);
    }

    public async Task<IdentityResult> ValidateResetPassword(
        User user,
        string token,
        string newPassword
    )
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task EnableTwoFactor(User user, bool status)
    {
        await _userManager.SetTwoFactorEnabledAsync(user, status);
    }
}
