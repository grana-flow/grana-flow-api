using PlanWise.Application.DTOs;

namespace PlanWise.Application.Interfaces;

public interface IManageAccountService
{
    Task<HttpResponseMessage> CreateAccount(UserVO model, string endpointPathToConfirmEmail);
    Task<HttpResponseMessage> SignIn(SignInVO model);
    Task<HttpResponseMessage> ConfirmEmail(string email, string token);
    Task<HttpResponseMessage> GenerateTwoFactorToken(string email);
    Task<HttpResponseMessage> ValidateTwoFactorToken(ValidateTwoFactor vo);
    Task<HttpResponseMessage> RequestForgetPassword(string email, string endpointPathToVerifyReset);
    Task<HttpResponseMessage> ValidateForgetPassword(ResetPasswordVO vo, string token);
}
