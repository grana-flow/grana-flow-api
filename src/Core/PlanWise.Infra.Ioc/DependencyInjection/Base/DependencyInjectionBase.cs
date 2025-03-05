using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
