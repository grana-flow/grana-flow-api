using Asp.Versioning;
using EmailServices.Interface;
using EmailServices.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GranaFlow.Application.Interfaces;
using GranaFlow.Application.Services;
using GranaFlow.Domain.Interfaces;
using GranaFlow.Infra.Data.Context;
using GranaFlow.Infra.Data.Repository;
using GranaFlow.Infra.Ioc.DependencyInjection.Base;
using RabbitMQServer.interfaces;
using RabbitMQServer.services;

namespace GranaFlow.Infra.Ioc.DependencyInjection
{
    public class DependencyInjectionIdentity : DependencyInjectionBase
    {
        public DependencyInjectionIdentity(IServiceCollection service, IConfiguration configuration)
            : base(service, configuration) { }

        public void ConfigurePasswordRules()
        {
            _serviceCollection?.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            });
        }

        public override void AddDbContext()
        {
            var connectionString = _configuration!
                .GetSection("ConnectionStrings")
                .GetSection("DefaultConnectionDb")
                .Value;

            _serviceCollection!.AddDbContext<ApplicationDbContextIdentity>(options =>
                options.UseNpgsql(connectionString)
            );
        }

        public void AddScopedAndDependencies()
        {
            _serviceCollection!.AddScoped<IManageAccountService, ManageAccountService>();
            _serviceCollection!.AddScoped<IManageAccountRepository, ManageAccountRepository>();
            _serviceCollection!.AddScoped<IEmailService, EmailService>();
            _serviceCollection!.AddSingleton<IRabbitMQMessageSender, RabbitMQMessageSender>();
        }

        public void AddApiVersioning()
        {
            _serviceCollection!
                .AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1);
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'V";
                    options.SubstituteApiVersionInUrl = true;
                });
        }
    }
}
