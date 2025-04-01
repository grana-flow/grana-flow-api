using System.Net;
using System.Text.Json;
using EmailServices.Contracts;
using EmailServices.Utils;
using Microsoft.Extensions.Configuration;
using GranaFlow.Application.Interfaces;
using GranaFlow.Domain.Contracts;
using GranaFlow.Domain.Entities;
using GranaFlow.Domain.Exceptions;
using GranaFlow.Domain.Interfaces;
using RabbitMQServer.contracts;
using RabbitMQServer.interfaces;
using GranaFlow.Application.JwtTokens;

namespace GranaFlow.Application.Services;

public class ManageAccountService : IManageAccountService
{
    private readonly IManageAccountRepository _repository;
    private readonly IRabbitMQMessageSender _rabbitMQServer;
    private readonly IConfiguration _configuration;

    public ManageAccountService(
        IManageAccountRepository repository,
        IRabbitMQMessageSender rabbitMqServer,
        IConfiguration configuration
    )
    {
        _repository = repository;
        _rabbitMQServer = rabbitMqServer;
        _configuration = configuration;
    }

    public async Task<HttpResponseMessage> CreateAccount(
        CreateUser model,
        string endpointPathToConfirmEmail
    )
    {
        var emailAlreadyExists = await _repository.EmailAlreadyExists(model.Email);
        if (emailAlreadyExists)
            throw new EmailAlreadyRegisteredException(model.Email);

        var user = User.CreateUser(model);
        var result = await _repository.CreateAccount(user, model.Password);

        if (result.Succeeded)
        {
            var token = await _repository.GenerateEmailConfirmationToken(user);
            var jsonReader = new JsonReader();
            var body = jsonReader.GetValue("Template", "EmailConfirmation");

            _rabbitMQServer.SendMessage(
                new DataServerRabbitMQ<EmailSendingDetails>(
                    hostName: _configuration.GetSection("RabbitMQServer:HostName").Value ?? "",
                    password: _configuration.GetSection("RabbitMQServer:Password").Value ?? "",
                    userName: _configuration.GetSection("RabbitMQServer:Username").Value ?? "",
                    virtualHost: _configuration.GetSection("RabbitMQServer:VirtualHost").Value
                        ?? "",
                    queueName: "confirmEmail-queue",
                    baseMessage: new EmailSendingDetails(
                        displayName: "GranaFlow",
                        body.Replace(
                            "linkToVerifyEmail",
                            $"{endpointPathToConfirmEmail}?token={token}&email={user.Email}"
                        ),
                        subject: "Confirm email",
                        isBodyHtml: true,
                        mailPriority: System.Net.Mail.MailPriority.High,
                        mailAddressesTo: new List<string> { user.Email! }
                    )
                )
            );

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            };
        }
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

    public async Task<HttpResponseMessage> SignIn(SignIn model)
    {
        var user = await _repository.FindByEmail(model.Email);

        if (!user!.EmailConfirmed)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.UnprocessableEntity
            };

        if (!await _repository.CheckPassword(user, model.Password))
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            };

        if (user.TwoFactorEnabled)
        {
            var token = await _repository.GenerateTwoFactorToken(user, "Email");
            var jsonReader = new JsonReader();
            var body = jsonReader.GetValue("Template", "TwoFactorToken");

            _rabbitMQServer.SendMessage(
                new DataServerRabbitMQ<EmailSendingDetails>(
                    hostName: _configuration.GetSection("RabbitMQServer:HostName").Value ?? "",
                    password: _configuration.GetSection("RabbitMQServer:Password").Value ?? "",
                    userName: _configuration.GetSection("RabbitMQServer:Username").Value ?? "",
                    virtualHost: _configuration.GetSection("RabbitMQServer:VirtualHost").Value
                        ?? "",
                    queueName: "2FA-queue",
                    baseMessage: new EmailSendingDetails(
                        displayName: "GranaFlow",
                        body.Replace("#token", token),
                        subject: "Verification code",
                        isBodyHtml: true,
                        mailPriority: System.Net.Mail.MailPriority.High,
                        mailAddressesTo: new List<string> { user.Email! }
                    )
                )
            );

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

        var authTokens = await GenerateAccessTokens(user);
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(authTokens),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> ConfirmEmail(string email, string token)
    {
        var user = await _repository.FindByEmail(email);

        var confirmEmail = await _repository.ConfirmEmail(user!, token);
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
            StatusCode = HttpStatusCode.OK
        };
    }

    public async Task<HttpResponseMessage> GenerateTwoFactorToken(string email)
    {
        var user = await _repository.FindByEmail(email);
        var token = await _repository.GenerateTwoFactorToken(user!, "Email");

        var jsonReader = new JsonReader();
        var body = jsonReader.GetValue("Template", "TwoFactorToken");
        _rabbitMQServer.SendMessage(
            new DataServerRabbitMQ<EmailSendingDetails>(
                hostName: _configuration.GetSection("RabbitMQServer:HostName").Value ?? "",
                password: _configuration.GetSection("RabbitMQServer:Password").Value ?? "",
                userName: _configuration.GetSection("RabbitMQServer:Username").Value ?? "",
                virtualHost: _configuration.GetSection("RabbitMQServer:VirtualHost").Value ?? "",
                queueName: "2FA-queue",
                baseMessage: new EmailSendingDetails(
                    displayName: "GranaFlow",
                    body.Replace("#token", token),
                    subject: "Verification code",
                    isBodyHtml: true,
                    mailPriority: System.Net.Mail.MailPriority.High,
                    mailAddressesTo: new List<string> { user!.Email! }
                )
            )
        );

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };
    }

    public async Task<HttpResponseMessage> ValidateTwoFactorToken(ValidateTwoFactorAuthentication model)
    {
        var user = await _repository.FindByEmail(model.Email);

        var isValid = await _repository.VerifyTwoFactorToken(user!, "Email", model.Token);
        if (!isValid)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            };

        if (!user!.TwoFactorEnabled)
        {
            await _repository.EnableTwoFactor(user, true);
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
        }

        var authTokens = await GenerateAccessTokens(user);
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(authTokens),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    public async Task<HttpResponseMessage> RequestForgetPassword(
        string email,
        string endpointPathToVerifyReset
    )
    {
        var user = await _repository.FindByEmail(email);
        var token = await _repository.GenerateForgetPasswordToken(user!);

        var jsonReader = new JsonReader();
        var body = jsonReader.GetValue("Template", "ForgetPassword");

        _rabbitMQServer.SendMessage(
            new DataServerRabbitMQ<EmailSendingDetails>(
                hostName: _configuration.GetSection("RabbitMQServer:HostName").Value ?? "",
                password: _configuration.GetSection("RabbitMQServer:Password").Value ?? "",
                userName: _configuration.GetSection("RabbitMQServer:Username").Value ?? "",
                virtualHost: _configuration.GetSection("RabbitMQServer:VirtualHost").Value ?? "",
                queueName: "forgetPassword-queue",
                baseMessage: new EmailSendingDetails(
                    displayName: "GranaFlow",
                    body.Replace("#token", $"{endpointPathToVerifyReset}?token={token}"),
                    subject: "Request password change",
                    isBodyHtml: true,
                    mailPriority: System.Net.Mail.MailPriority.High,
                    mailAddressesTo: new List<string> { user!.Email! }
                )
            )
        );

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };
    }

    public async Task<HttpResponseMessage> ValidateForgetPassword(ResetPassword model, string token)
    {
        var user = await _repository.FindByEmail(model.Email);
        var decodedToken = WebUtility.UrlDecode(token);
        var resetPasswordResult = await _repository.ValidateResetPassword(
            user!,
            decodedToken,
            model.Password
        );

        if (resetPasswordResult.Succeeded)
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
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

    public async Task<HttpResponseMessage> VerifyRefreshToken(RefreshToken model)
    {
        var user = await _repository.FindByEmail(model.Email);
        var isValid = await _repository.VerifyUserToken(user, _configuration["JwtSettings:RefreshToken:LoginProvaider"]!, _configuration["JwtSettings:RefreshToken:Name"]!, model.Refresh);

        if (!isValid)
            return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };

        var authTokens = await GenerateAccessTokens(user);
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(authTokens),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        };
    }

    private async Task<AuthTokenResponse> GenerateAccessTokens(User user)
    {
        var accessToken = TokenService.GenerateAccessToken(_configuration, user);
        var authToken = new AuthTokenResponse(
            accessToken: accessToken,
            refreshToken: TokenService.GenerateRefreshToken(_configuration, accessToken),
            expiration: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TokenExpirationMinutes"]!)));
        await _repository.SetAuthenticationToken(user, _configuration["JwtSettings:RefreshToken:LoginProvaider"]!, _configuration["JwtSettings:RefreshToken:Name"]!, authToken.RefreshToken);
        return authToken;
    }
}
