using GranaFlow.Infra.Ioc.Configs.Swagger.ExampleResponse;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace GranaFlow.Infra.Ioc.Configs.Swagger;

public static class SwaggerConfigs
{
    public static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            opt.EnableAnnotations();
            opt.ExampleFilters();
            opt.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "GranaFlow.API",
                    Version = "v1",
                    Description = "API's do sistema GranaFlow",
                    License = new OpenApiLicense
                    {
                        Name = "Repositório",
                        Url = new Uri("https://github.com/leeo-sf/identity.git")
                    }
                }
            );
        });
        services.AddSwaggerExamplesFromAssemblyOf<SignInResponseExample>();
    }

    public static void UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "GranaFlow.API v1");
            });
        }
    }
}
