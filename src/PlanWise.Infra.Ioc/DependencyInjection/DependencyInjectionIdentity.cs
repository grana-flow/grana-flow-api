using AutoMapper;
using EmailServices.Interface;
using EmailServices.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlanWise.Application.Interfaces;
using PlanWise.Application.Mappings;
using PlanWise.Application.Services;
using PlanWise.Domain.Interfaces;
using PlanWise.Infra.Data.Context;
using PlanWise.Infra.Data.Repository;
using PlanWise.Infra.Ioc.DependencyInjection.Base;

namespace PlanWise.Infra.Ioc.DependencyInjection
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
            IMapper mapper = DomainToMappingUser.RegisterMaps().CreateMapper();
            _serviceCollection!.AddSingleton(mapper);
            _serviceCollection!.AddScoped<IManageAccountService, ManageAccountService>();
            _serviceCollection!.AddScoped<IManageAccountRepository, ManageAccountRepository>();
            _serviceCollection!.AddScoped<IEmailService, EmailService>();
        }
    }
}
