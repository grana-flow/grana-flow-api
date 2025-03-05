using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PlanWise.Application.DTOs;
using PlanWise.Application.Interfaces;

namespace PlanWise.Presentation.Endpoints;

public static class TwoFactorAuthenticationEndpoints
{
    public static void AddTwoFactorAuthenticationEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/2FA").WithTags("2FA");

        group
            .MapGet("verify/email", CheckEmail)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                return new(opt)
                {
                    Summary = "Confirm e-mail",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Confirm e-mail",
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            ["message"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Example = new OpenApiString("Email confirmed")
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        ["400"] = new OpenApiResponse
                        {
                            Description = "Possible reasons:\n- Invalid e-mail\n- Invalid token"
                        }
                    }
                };
            });
        group
            .MapGet("send/token", GenerateQrCodeTwoFactor)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                return new(opt)
                {
                    Summary = "Send e-mail confirmation token",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "E-mail sent",
                            Content =
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            ["message"] = new OpenApiSchema
                                            {
                                                Type = "string",
                                                Example = new OpenApiString("Code sent")
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        ["400"] = new OpenApiResponse { Description = "E-mail not found" }
                    }
                };
            });
        group
            .MapPost("verify/token", ValidateTwoFactorToken)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                return new(opt)
                {
                    Summary = "Authenticates confirmation token",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse { Description = "Generated JWT token", },
                        ["401"] = new OpenApiResponse { Description = "Invalid token" },
                        ["400"] = new OpenApiResponse { Description = "E-mail not found" }
                    }
                };
            });
    }

    public static async Task<IResult> CheckEmail(
        [FromServices] IManageAccountService service,
        [FromQuery] string email,
        [FromQuery] string token
    )
    {
        try
        {
            var confirmedEmail = await service.ConfirmEmail(email, token);

            return Results.Content(
                await confirmedEmail.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)confirmedEmail.StatusCode
            );
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    public static async Task<IResult> GenerateQrCodeTwoFactor(
        [FromServices] IManageAccountService service,
        [FromHeader] string email
    )
    {
        try
        {
            var token = await service.GenerateTwoFactorToken(email);

            return Results.Content(
                await token.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)token.StatusCode
            );
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    public static async Task<IResult> ValidateTwoFactorToken(
        [FromServices] IManageAccountService service,
        [FromBody] ValidateTwoFactor vo
    )
    {
        try
        {
            var validated = await service.ValidateTwoFactorToken(vo);

            return Results.Content(
                await validated.Content.ReadAsStringAsync(),
                "application/json",
                Encoding.UTF8,
                (int)validated.StatusCode
            );
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}
