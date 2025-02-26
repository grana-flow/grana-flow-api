using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PlanWise.Application.DTOs;
using PlanWise.Application.Interfaces;

namespace PlanWise.Presentation.Endpoints;

public static class TwoFactorAuthenticationEndpoints
{
    public static void AddTwoFactorAuthenticationEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/2FA").WithTags("2FA");

        group.MapGet("verify/email", CheckEmail);
        group.MapGet("send/token", GenerateQrCodeTwoFactor);
        group.MapPost("verify/token", ValidateTwoFactorToken);
    }

    public static async Task<IActionResult> CheckEmail(
        [FromServices] IManageAccountService service,
        [FromQuery] string email, 
        [FromQuery] string token
    )
    {
        try
        {
            var confirmedEmail = await service.ConfirmEmail(email, token);

            return new ContentResult
            {
                Content = await confirmedEmail.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)confirmedEmail.StatusCode
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }

    public static async Task<IActionResult> GenerateQrCodeTwoFactor(
        [FromServices] IManageAccountService service,
        [FromHeader] string email
    )
    {
        try
        {
            var token = await service.GenerateTwoFactorToken(email);

            return new ContentResult
            {
                Content = await token.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)token.StatusCode
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }

    public static async Task<IActionResult> ValidateTwoFactorToken(
        [FromServices] IManageAccountService service,
        [FromBody] ValidateTwoFactor vo
    )
    {
        try
        {
            var validated = await service.ValidateTwoFactorToken(vo);

            return new ContentResult
            {
                Content = await validated.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)validated.StatusCode
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }
}
