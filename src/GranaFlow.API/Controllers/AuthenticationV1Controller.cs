using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using GranaFlow.Application.Interfaces;
using GranaFlow.Domain.Contracts;
using GranaFlow.Domain.Exceptions;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;
using GranaFlow.Infra.Ioc.Configs.Swagger.ExampleResponse;

namespace GranaFlow.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
[ControllerName("Auth")]
public class AuthenticationV1Controller : Controller
{
    private readonly IManageAccountService _service;

    public AuthenticationV1Controller(IManageAccountService service)
    {
        _service = service;
    }

    [HttpPost("sign-in")]
    [SwaggerOperation("Realizar login")]
    [SwaggerResponse(StatusCodes.Status200OK, "Autenticação bem sucedida, tokens gerados", typeof(AuthTokenResponse))]
    [SwaggerResponse(StatusCodes.Status206PartialContent, "Obrigatório autenticação de dois fatores")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "E-mail ou senha incorreto")]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "E-mail não confirmado")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> SignIn([FromBody] SignIn model)
    {
        try
        {
            var authenticatedUser = await _service.SignIn(model);

            return Results.Content(
                await authenticatedUser.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)authenticatedUser.StatusCode
            );
        }
        catch (EmailNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sign-up")]
    [SwaggerOperation("Cadastro no sistema")]
    [SwaggerResponse(StatusCodes.Status201Created, "Cadastro realizado")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Senha deve conter 6 caracteres.| Senha deve conter 1 caractere especial. | Senha deve conter alphanumérico. | Erro levantado pelo ASP.NET Identity (username em uso) ETC.")]
    public async Task<IResult> SignUp([FromBody] CreateUser model)
    {
        try
        {
            var baseUrl =
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/2FA/verify/email";
            var registeredUser = await _service.CreateAccount(model, baseUrl);
            return Results.Content(
                await registeredUser.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)registeredUser.StatusCode
            );
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("forget/password")]
    [SwaggerOperation("Solicita cadastro de nova senha")]
    [SwaggerResponse(StatusCodes.Status200OK, "Link para alteração de senha enviado por e-mail")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> ForgetPassword([FromHeader] string email)
    {
        try
        {
            var baseUrl =
            $"{Request.Scheme} :// {Request.Host} {Request.PathBase}/api/v1/auth/validate/password/change";
            var forgetPassword = await _service.RequestForgetPassword(email, baseUrl);
            return Results.Content(
            await forgetPassword.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)forgetPassword.StatusCode
            );
        }
        catch (EmailNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("validate/password/change")]
    [SwaggerOperation("Valida alteração da senha")]
    [SwaggerResponse(StatusCodes.Status200OK, "Senha alterada com sucesso")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Token inválido. | Senha não corresponde ao padrão")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> ValidateForgetPassword([FromBody] ResetPassword model, [FromQuery] string token)
    {
        try
        {
            var passwordChanged = await _service.ValidateForgetPassword(model, token);

            return Results.Content(
                await passwordChanged.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)passwordChanged.StatusCode
            );
        }
        catch (EmailNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [SwaggerOperation("Solicita novo AccessToken com base em um RefreshToken")]
    [SwaggerResponse(StatusCodes.Status200OK, "Token válido, novos tokens gerados", typeof(AuthTokenResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Refresh token inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> Refresh([FromBody] RefreshToken model)
    {
        try
        {
            var resultRefreshToken = await _service.VerifyRefreshToken(model);

            return Results.Content(
                await resultRefreshToken.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)resultRefreshToken.StatusCode
            );
        }
        catch (EmailNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }
}
