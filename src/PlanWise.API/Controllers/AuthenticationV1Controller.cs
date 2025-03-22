using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PlanWise.Application.Interfaces;
using PlanWise.Domain.Contracts;
using PlanWise.Domain.Exceptions;
using System.Text;

namespace PlanWise.API.Controllers;

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
}
