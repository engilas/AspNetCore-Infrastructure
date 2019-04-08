using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Infrastructure.Background;
using Infrastructure.Configuration;
using Infrastructure.Events;
using Infrastructure.Logging;
using Infrastructure.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Infrastructure.Tests.Base
{
    public class UnitTestBase : TestBase
    {
        private QueuedHostedService _queuedHostedService;

        protected UnitTestBase(Assembly startupAssembly)
        {
            string root;
            if (startupAssembly != null)
            {
                root = Helpers.GetProjectPath(Path.Combine("src"), startupAssembly);
            }
            else
            {
                root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            Configuration = AppConfigurations.Get(root, null, "UnitTest", substituteVariables: true);
            LogFactory.Initialize(Configuration);

            ServiceProvider = BuildServices(ConfigureServices);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScoped<IHostingEnvironment>(s => new HostingEnvironment{EnvironmentName = "UnitTest"});
        }

        protected IServiceProvider BuildServices(Action<IServiceCollection> configureAction = null)
        {
            var serviceCollection = new ServiceCollection();
            var startup = new DefaultStartup(Configuration);
            startup.ConfigureServices(serviceCollection);
            serviceCollection.AddLogging(builder => builder.AddSerilog());
            configureAction?.Invoke(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        protected void StartEventProcessors()
        {
            EventExtensions.StartEventProcessors(ServiceProvider);
        }

        protected void StartQueuedService()
        {
            _queuedHostedService = new QueuedHostedService(ServiceProvider.GetService<IBackgroundTaskManager>(), ServiceProvider.GetService<ILogger<QueuedHostedService>>());
            _queuedHostedService.StartAsync(CancellationToken.None);
        }
    }
}