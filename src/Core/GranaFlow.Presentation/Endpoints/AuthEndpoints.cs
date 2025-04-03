using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PlanWise.Application.Interfaces;
using PlanWise.Domain.Contracts;
using PlanWise.Domain.Exceptions;

namespace PlanWise.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void AddAuthEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group
            .MapPost("sign-in", SignIn)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                return new(opt)
                {
                    Summary = "Authenticates",
                    Responses =
                    {
                        //add status code 200
                        ["206"] = new OpenApiResponse
                        {
                            Description = "Two-factor authentication required"
                        },
                        ["404"] = new OpenApiResponse { Description = "E-mail nor found" },
                        ["400"] = new OpenApiResponse
                        {
                            Description =
                                "Possible reasons:\n- E-mail not confirmed\n- Invalid authentication (Invalid e-mail or password)"
                        }
                    }
                };
            });
        group
            .MapPost("sign-up", SignUp)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                opt.Responses.Remove("200");

                return new(opt)
                {
                    Summary = "Create account",
                    Responses =
                    {
                        ["201"] = new OpenApiResponse
                        {
                            Description = "User created",
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
                                                Example = new OpenApiString("Message user created")
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        ["400"] = new OpenApiResponse
                        {
                            Description =
                                "Possible reasons:\n- E-mail already registered\n- Username already registered",
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
                                                Example = new OpenApiString("Message error")
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    }
                };
            });
        group
            .MapGet("forget/password", ForgetPassword)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                opt.Responses.Remove("200");
                return new(opt)
                {
                    Summary = "Request password change",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Successful request",
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
                                                Example = new OpenApiString(
                                                    "Link to change password sent to registered email"
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        ["404"] = new OpenApiResponse { Description = "E-mail not found" },
                    }
                };
            });
        group
            .MapPost("validate/password/change", ValidateForgetPassword)
            .MapToApiVersion(1)
            .WithOpenApi(opt =>
            {
                opt.Responses.Remove("200");
                return new(opt)
                {
                    Summary = "Password change",
                    Responses =
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Password change",
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
                                                Example = new OpenApiString(
                                                    "Password has been changed"
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        ["404"] = new OpenApiResponse { Description = "E-mail not found" },
                        ["400"] = new OpenApiResponse
                        {
                            Description =
                                "Possible reasons:\n- Invalid token\n- Password not required"
                        }
                    }
                };
            });
    }

    public static async Task<IResult> SignIn(
        [FromServices] IManageAccountService service,
        [FromBody] SignIn model
    )
    {
        try
        {
            var authenticatedUser = await service.SignIn(model);

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

    public static async Task<IResult> SignUp(
        HttpRequest httpRequest,
        [FromServices] IManageAccountService service,
        [FromBody] CreateUser model
    )
    {
        try
        {
            var baseUrl =
                $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/api/v1/2FA/verify/email";
            var registeredUser = await service.CreateAccount(model, baseUrl);

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

    public static async Task<IResult> ForgetPassword(
        HttpRequest httpRequest,
        [FromServices] IManageAccountService service,
        [FromHeader] string email
    )
    {
        try
        {
            var baseUrl =
                $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/api/v1/auth/validate/password/change";
            var forgetPassword = await service.RequestForgetPassword(email, baseUrl);

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

    public static async Task<IResult> ValidateForgetPassword(
        [FromServices] IManageAccountService service,
        [FromBody] ResetPassword vo,
        [FromQuery] string token
    )
    {
        try
        {
            var passwordChanged = await service.ValidateForgetPassword(vo, token);

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
