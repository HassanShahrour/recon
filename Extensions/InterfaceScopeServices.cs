using DinkToPdf;
using DinkToPdf.Contracts;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.DatabaseHelper.Repositories;
using Reconova.Core.Utilities;
using Reconova.Services;

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
            services.AddScoped<IFollowRepository, FollowRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IScheduleScansRepository, ScheduleScansRepository>();
            services.AddScoped<ScanUtility>();
            services.AddScoped<UserUtility>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddHostedService<DailyScanScheduler>();
            services.AddHostedService<PlanExpiryCheckerService>();

            return services;
        }
    }
}
