using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using PlanWise.Presentation.Endpoints;

namespace PlanWise.Presentation;

public static class DependencyInjection
{
    public static IEndpointRouteBuilder AddEndPoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            //.HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();
        RouteGroupBuilder groupBuilder = app.MapGroup("api/v{apiVersion:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        groupBuilder.AddAuthEndPoints();
        groupBuilder.AddTwoFactorAuthenticationEndPoints();

        return app;
    }
}
