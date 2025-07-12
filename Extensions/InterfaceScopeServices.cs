using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.DatabaseHelper.Repositories;
using Reconova.Core.Utilities;

namespace Reconova.Extensions
{
    public static class InterfaceScopeServices
    {
        public static IServiceCollection AddInterfaceScopeServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IScanRepository, ScanRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IToolsRepository, ToolsRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ScanUtility>();
            services.AddScoped<UserUtility>();
            services.AddScoped<AIUtility>();

            return services;
        }
    }
}
