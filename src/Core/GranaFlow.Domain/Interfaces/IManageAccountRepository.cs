using GranaFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GranaFlow.Domain.Interfaces;

public interface IManageAccountRepository
{
    Task<IdentityResult> CreateAccount(User user, string password);
    Task<User?> FindByEmail(string email);
    Task<bool> EmailAlreadyExists(string email);
    Task<bool> IsEmailConfirmed(User user);
    Task<bool> CheckPassword(User user, string password);
    Task<string> GenerateEmailConfirmationToken(User user);
    Task<IdentityResult> ConfirmEmail(User user, string token);
    Task<string> GenerateTwoFactorToken(User user, string provaider);
    Task<string> GenerateForgetPasswordToken(User user);
    Task<bool> VerifyTwoFactorToken(User user, string provaider, string token);
    Task<IdentityResult> ValidateResetPassword(User user, string token, string newPassword);
    Task EnableTwoFactor(User user, bool status);
    Task SetAuthenticationToken(
        User user,
        string loginProvaider,
        string tokenName,
        string tokenValue
    );
    Task<bool> VerifyUserToken(
        User user,
        string loginProvaider,
        string tokenName,
        string tokenValue
    );
}
