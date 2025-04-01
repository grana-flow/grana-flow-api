using GranaFlow.Domain.Contracts;

namespace GranaFlow.Application.Interfaces;

public interface IManageAccountService
{
    Task<HttpResponseMessage> CreateAccount(CreateUser model, string endpointPathToConfirmEmail);
    Task<HttpResponseMessage> SignIn(SignIn model);
    Task<HttpResponseMessage> ConfirmEmail(string email, string token);
    Task<HttpResponseMessage> GenerateTwoFactorToken(string email);
    Task<HttpResponseMessage> ValidateTwoFactorToken(ValidateTwoFactorAuthentication model);
    Task<HttpResponseMessage> RequestForgetPassword(string email, string endpointPathToVerifyReset);
    Task<HttpResponseMessage> ValidateForgetPassword(ResetPassword model, string token);
    Task<HttpResponseMessage> VerifyRefreshToken(RefreshToken model);
}
