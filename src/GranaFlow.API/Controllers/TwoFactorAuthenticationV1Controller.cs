using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using GranaFlow.Application.Interfaces;
using GranaFlow.Domain.Contracts;
using GranaFlow.Domain.Exceptions;
using System.Text;

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
