using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PlanWise.Infra.Ioc.DependencyInjection.Base
{
    public abstract class DependencyInjectionBase
    {
        protected readonly IServiceCollection? _serviceCollection;
        protected readonly IConfiguration? _configuration;

        public DependencyInjectionBase(
            IServiceCollection serviceCollection,
            IConfiguration configuration
        )
        {
            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        public abstract void AddDbContext();

        public void ConfigAuthentication()
        {
            var jwtSettings = _configuration!.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            _serviceCollection!.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });
        }
    }
}
