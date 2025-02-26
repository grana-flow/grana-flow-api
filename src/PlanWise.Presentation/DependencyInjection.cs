using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PlanWise.Application.DTOs;
using PlanWise.Application.Interfaces;
using PlanWise.Presentation.Endpoints;

namespace PlanWise.Presentation;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder AddEndPoints(this IEndpointRouteBuilder app)
    {
        app.AddAuthEndPoints();
        app.AddTwoFactorAuthenticationEndPoints();

        return app;
    }
}
