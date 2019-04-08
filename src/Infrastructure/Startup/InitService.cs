using System;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Startup
{
    internal class InitService
    {
        public static void InitServices(IServiceProvider serviceProvider)
        {
            try
            {
                var services = serviceProvider.GetServices<IInitService>();
                services.AsyncForeach(service => service.Init()).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<InitService>>();
                logger.LogError(ex, "Failed on service initialization");
            }
        }
    }
}
