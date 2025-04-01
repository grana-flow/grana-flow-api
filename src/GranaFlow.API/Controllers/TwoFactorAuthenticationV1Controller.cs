using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using GranaFlow.Application.Interfaces;
using GranaFlow.Domain.Contracts;
using GranaFlow.Domain.Exceptions;
using System.Text;
using GranaFlow.Infra.Ioc.Configs.Swagger.ExampleResponse;
using Swashbuckle.AspNetCore.Annotations;

namespace GranaFlow.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/two-factor")]
[ApiVersion("1.0")]
[ControllerName("2FA")]
public class TwoFactorAuthenticationV1Controller : Controller
{
    private readonly IManageAccountService _service;

    public TwoFactorAuthenticationV1Controller(IManageAccountService service)
    {
        _service = service;
    }

    [HttpGet("verify/email")]
    [SwaggerOperation("Valida confirmação de e-mail")]
    [SwaggerResponse(StatusCodes.Status200OK, "E-mail confirmado")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Confirmação inválida (token incorreto)")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> CheckEmail([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            var confirmedEmail = await _service.ConfirmEmail(email, token);

            return Results.Content(
                await confirmedEmail.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)confirmedEmail.StatusCode
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

    [HttpGet("send/token")]
    [SwaggerOperation("Envia token de auth 2 fatores")]
    [SwaggerResponse(StatusCodes.Status200OK, "Código enviado para e-mail")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> GenerateTokenTwoFactor([FromHeader] string email)
    {
        try
        {
            var token = await _service.GenerateTwoFactorToken(email);

            return Results.Content(
                await token.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)token.StatusCode
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

    [HttpPost("verify/token")]
    [SwaggerOperation("Valida código de auth 2 fatores")]
    [SwaggerResponse(StatusCodes.Status200OK, "Token gerado", typeof(AuthTokenResponse))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token inválido")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "E-mail não encontrado")]
    public async Task<IResult> ValidateTwoFactorToken([FromBody] ValidateTwoFactorAuthentication model)
    {
        try
        {
            var validated = await _service.ValidateTwoFactorToken(model);

            return Results.Content(
                await validated.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)validated.StatusCode
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
}
