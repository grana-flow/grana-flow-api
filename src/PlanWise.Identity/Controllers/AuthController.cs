using Microsoft.AspNetCore.Mvc;
using PlanWise.Application.DTOs;
using PlanWise.Application.Interfaces;

namespace ReceitaAI.Identity.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IManageAccountService _manageUserService;

    public AuthController(IManageAccountService manageUserService)
    {
        _manageUserService = manageUserService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserVO model)
    {
        var baseUrl =
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/Auth/verifyEmail";
        var registeredUser = await _manageUserService.CreateAccount(model, baseUrl);

        return new ContentResult
        {
            Content = await registeredUser.Content.ReadAsStringAsync(),
            ContentType = "application/json",
            StatusCode = (int)registeredUser.StatusCode
        };
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> Authenticate([FromBody] SignInVO model)
    {
        try
        {
            var authenticatedUser = await _manageUserService.SignIn(model);

            return new ContentResult
            {
                Content = await authenticatedUser.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)authenticatedUser.StatusCode
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("verifyEmail")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            var confirmedEmail = await _manageUserService.ConfirmEmail(email, token);

            return new ContentResult
            {
                Content = await confirmedEmail.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)confirmedEmail.StatusCode
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("send/2fa/token")]
    public async Task<IActionResult> GenerateQrCodeTwoFactor([FromHeader] string email)
    {
        try
        {
            var token = await _manageUserService.GenerateTwoFactorToken(email);

            return new ContentResult
            {
                Content = await token.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)token.StatusCode
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify/2fa/token")]
    public async Task<IActionResult> ValidateTwoFactorToken([FromBody] ValidateTwoFactor vo)
    {
        try
        {
            var validated = await _manageUserService.ValidateTwoFactorToken(vo);

            return new ContentResult
            {
                Content = await validated.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)validated.StatusCode
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("request/password-change")]
    public async Task<IActionResult> ForgetPassword([FromHeader] string email)
    {
        try
        {
            var baseUrl =
                $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/v1/Auth/validate/password-change";
            var forgetPassword = await _manageUserService.RequestForgetPassword(email, baseUrl);

            return new ContentResult
            {
                Content = await forgetPassword.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)forgetPassword.StatusCode
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("validate/password-change")]
    public async Task<IActionResult> ValidateForgetPassword([FromBody] ResetPasswordVO vo, [FromQuery] string token)
    {
        try
        {
            var passwordChanged = await _manageUserService.ValidateForgetPassword(vo, token);

            return new ContentResult
            {
                Content = await passwordChanged.Content.ReadAsStringAsync(),
                ContentType = "application/json",
                StatusCode = (int)passwordChanged.StatusCode
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
