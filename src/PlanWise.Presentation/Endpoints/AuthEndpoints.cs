using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Org.BouncyCastle.Asn1.Ocsp;
using PlanWise.Application.DTOs;
using PlanWise.Application.Interfaces;

namespace PlanWise.Presentation.Endpoints;
public static class AuthEndpoints
{
    public static void AddAuthEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("sign-in", SignIn);
        group.MapPost("sign-up", SignUp);
        group.MapGet("password/change", ForgetPassword);
        group.MapPost("validate/password/change", ValidateForgetPassword);
    }

    public static async Task<IActionResult> SignIn(
        [FromServices] IManageAccountService service, 
        [FromBody] SignInVO model
    )
    {
        try
        {
            var authenticatedUser = await service.SignIn(model);

            return new ContentResult
            {
                Content = await authenticatedUser.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)authenticatedUser.StatusCode
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }

    public static async Task<IActionResult> SignUp(
        HttpRequest httpRequest,
        [FromServices] IManageAccountService service,
        [FromBody] UserVO model
    )
    {
        var baseUrl =
            $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/api/v1/Auth/verifyEmail";
        var registeredUser = await service.CreateAccount(model, baseUrl);

        return new ContentResult
        {
            Content = await registeredUser.Content.ReadAsStringAsync(),
            ContentType = "application/json",
            StatusCode = (int)registeredUser.StatusCode
        };
    }

    public static async Task<IActionResult> ForgetPassword(
        HttpRequest httpRequest,
        [FromServices] IManageAccountService service,
        [FromHeader] string email
    )
    {
        try
        {
            var baseUrl =
                $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/api/v1/Auth/validate/password-change";
            var forgetPassword = await service.RequestForgetPassword(email, baseUrl);

            return new ContentResult
            {
                Content = await forgetPassword.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)forgetPassword.StatusCode
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }

    public static async Task<IActionResult> ValidateForgetPassword(
        [FromServices] IManageAccountService service,
        [FromBody] ResetPasswordVO vo,
        [FromQuery] string token
    )
    {
        try
        {
            var passwordChanged = await service.ValidateForgetPassword(vo, token);

            return new ContentResult
            {
                Content = await passwordChanged.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)passwordChanged.StatusCode
            };
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { message = ex.Message });
        }
    }
}
