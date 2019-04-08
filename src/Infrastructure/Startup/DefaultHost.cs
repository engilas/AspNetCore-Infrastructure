using System;
using System.IO;
using System.Reflection;
using Infrastructure.Configuration;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Startup
{
    public class StartupOptions
    {
        private string[] _args;
        private Action<KestrelServerOptions> _kestrelOptions;
        private Action<WebHostBuilderContext, IConfigurationBuilder> _configureDelegate;

        internal string[] Args
        {
            get => _args ?? new string[0];
            private set => _args = value;
        }

        internal Action<KestrelServerOptions> KestrelOptions
        {
            get => _kestrelOptions ?? (_ => {});
            private set => _kestrelOptions = value;
        }

        internal Action<WebHostBuilderContext, IConfigurationBuilder> ConfigureDelegate
        {
            get => _configureDelegate ?? ((_, __) => {});
            private set => _configureDelegate = value;
        }

        public StartupOptions AddArgs(string[] args)
        {
            Args = args;
            return this;
        }

        public StartupOptions ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            ConfigureDelegate = configureDelegate;
            return this;
        }

        public StartupOptions UseKestrel(Action<KestrelServerOptions> kestrelOptions)
        {
            KestrelOptions = kestrelOptions;
            return this;
        }

        public void Run() => DefaultHost.RunHost(this);
        public void Run<TStartup>() where TStartup : class => DefaultHost.RunHost<TStartup>(this);
    }

    public class DefaultHost
    {
        public static void RunHost(StartupOptions options = null) => RunHost<DefaultStartup>(options);

        public static StartupOptions CreateHostBuilder() => new StartupOptions();

        public static void RunHost<TStartup>(StartupOptions options = null)
        where TStartup: class
        {
            options = options ?? new StartupOptions();

            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(root);

            var configuration = AppConfigurations.Get(root, options.Args,
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), substituteVariables: true);

            LogFactory.Initialize(configuration);

            try
            {
                var host = new WebHostBuilder()
                    .UseKestrel(options.KestrelOptions)
                    .UseContentRoot(root)
                    .UseStartup<TStartup>()
                    .UseSerilog()
                    .ConfigureAppConfiguration((builder, config) =>
                    {
                        AppConfigurations.BuildConfiguration(root, options.Args, builder.HostingEnvironment.EnvironmentName,
                            builder: config);
                        options.ConfigureDelegate(builder, config);
                    })
                    .ConfigureServices((builder, services) =>
                    {
                        AppConfigurations.SubstituteVariables(builder.Configuration);
                    })
                    .UseUrls(configuration.GetSection("urls").Get<string>().Split(";"))
                    .Build();

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
