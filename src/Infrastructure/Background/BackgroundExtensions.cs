using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Background
{
    public static class BackgroundExtensions
    {
        /// <summary>
        ///     Adds a background task services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBackground(this IServiceCollection services)
        {
            services.AddHostedService<QueuedHostedService>();

            services.AddSingleton<IBackgroundTaskQueue, IBackgroundTaskManager, BackgroundTaskQueue>();

            return services;
        }

        public static IServiceCollection AddTimedService<T>(this IServiceCollection services)
        where T : class, ITimedService
        {
            services.AddScoped<T>();
            services.AddHostedService<BackgroundTimedService<T>>();

            return services;
        }
    }
}