using System.Net;
using System.Text.Json;
using AutoMapper;
using EmailServices.Contracts;
using EmailServices.Interface;
using EmailServices.Utils;
using Microsoft.Extensions.Configuration;
using PlanWise.Application.DTOs;
using PlanWise.Application.Interfaces;
using PlanWise.Domain.Entities;
using PlanWise.Domain.Interfaces;

namespace PlanWise.Application.Services;

public class ManageAccountService : IManageAccountService
{
    private readonly IManageAccountRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public ManageAccountService(
        IManageAccountRepository repository,
        IEmailService emailService,
        IMapper mapper,
        IConfiguration config
    )
    {
        _repository = repository;
        _emailService = emailService;
        _mapper = mapper;
        _config = config;
    }

    public async Task<HttpResponseMessage> CreateAccount(
        UserVO model,
        string endpointPathToConfirmEmail
    )
    {
        var user = _mapper.Map<User>(model);

        var result = await _repository.CreateAccount(user, model.Password);

        if (result.Succeeded)
        {
            var token = await _repository.GenerateEmailConfirmationToken(user);

            _ = Task.Run(() =>
            {
                var jsonReader = new JsonReader();
                var body = jsonReader.GetValue("Template", "EmailConfirmation");
                _ = SendEmail(
                    "Confirm email",
                    body.Replace(
                        "linkToVerifyEmail",
                        $"{endpointPathToConfirmEmail}?token={token}&email={user.Email}"
                    ),
                    new List<string> { user.Email }
                );
            });

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new { message = "User registered. Check your email to confirm it." }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };
        }
        else
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new { message = result.Errors.FirstOrDefault()?.Description }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };
        }
    }

    public async Task<HttpResponseMessage> SignIn(SignInVO model)
    {
        var user = await _repository.FindByEmail(model.Email);

        if (!user.EmailConfirmed)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new { message = "Email not yet confirmed. Check your email inbox." }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        if (!await _repository.CheckPassword(user, model.Password))
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(
                    JsonSerializer.Serialize(new { message = "Invalid authentication." }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        if (user.TwoFactorEnabled)
        {
            var token = await _repository.GenerateTwoFactorToken(user, "Email");
            _ = Task.Run(() =>
            {
                var jsonReader = new JsonReader();
                var body = jsonReader.GetValue("Template", "TwoFactorToken");
                _ = SendEmail(
                    "Verification code",
                    body.Replace("#token", token),
                    new List<string> { user.Email }
                );
            });

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.PartialContent,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new
                        {
                            requires2FA = true,
                            message = "Two-step verification code sent to your email."
                        }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };
        }

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(new { message = "Generate token." }),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> ConfirmEmail(string email, string token)
    {
        var user = await _repository.FindByEmail(email);

        if (user is null)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new { message = "Invalid Email Confirmation Request." }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        var confirmEmail = await _repository.ConfirmEmail(user, token);
        if (!confirmEmail.Succeeded)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new { message = "Invalid Email Confirmation Request." }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(new { message = "Email confirmed!" }),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> GenerateTwoFactorToken(string email)
    {
        var user = await _repository.FindByEmail(email);
        var token = await _repository.GenerateTwoFactorToken(user, "Email");

        var jsonReader = new JsonReader();
        var body = jsonReader.GetValue("Template", "TwoFactorToken");
        await SendEmail(
            "Verification code",
            body.Replace("#token", token),
            new List<string> { user.Email }
        );

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(new { message = "Code sent to email." }),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> ValidateTwoFactorToken(ValidateTwoFactor vo)
    {
        var user = await _repository.FindByEmail(vo.Email);

        var isValid = await _repository.VerifyTwoFactorToken(user, "Email", vo.Token);
        if (!isValid)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(
                    JsonSerializer.Serialize(new { message = "Invalid token." }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        // TODO: gerar token JWT
        if (!user.TwoFactorEnabled)
            await _repository.EnableTwoFactor(user, true);

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(new { message = "token gerado." }),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> RequestForgetPassword(string email, string endpointPathToVerifyReset)
    {
        var user = await _repository.FindByEmail(email);
        var token = await _repository.GenerateForgetPasswordToken(user);

        _ = Task.Run(() =>
        {
            var jsonReader = new JsonReader();
            var body = jsonReader.GetValue("Template", "ForgetPassword");
            _ = SendEmail(
                "Request password change",
                body.Replace("#token", $"{endpointPathToVerifyReset}?token={token}"),
                new List<string> { user.Email }
            );
        });

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(
                    new { message = "Password reset link sent to registered email." }
                ),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> ValidateForgetPassword(ResetPasswordVO vo, string token)
    {
        var user = await _repository.FindByEmail(vo.Email);
        var resetPasswordResult = await _repository.ValidateResetPassword(
            user,
            token,
            vo.Password
        );

        if (resetPasswordResult.Succeeded)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonSerializer.Serialize(new { message = "Your password has been changed." }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(
                JsonSerializer.Serialize(
                    new { message = resetPasswordResult.Errors.FirstOrDefault()?.Description }
                ),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    private async Task SendEmail(string subject, string body, List<string> mailAddressesTo)
    {
        await _emailService.SendEmail(
            new EmailSendingDetails(
                username: _config
                    .GetSection("EmailService")
                    .GetSection("Credentials")
                    .GetSection("Username")
                    .Value ?? throw new ApplicationException("Not seted username."),
                passwordApp: _config
                    .GetSection("EmailService")
                    .GetSection("Credentials")
                    .GetSection("PasswordApp")
                    .Value ?? throw new ApplicationException("Not seted password app."),
                displayName: "PlanWise",
                body: body,
                subject: subject,
                isBodyHtml: true,
                mailPriority: System.Net.Mail.MailPriority.High,
                mailAddressesTo: mailAddressesTo
            )
        );
    }
}
