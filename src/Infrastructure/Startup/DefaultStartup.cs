using System.Diagnostics;
using Dapper;
using Infrastructure.Asp;
using Infrastructure.Controllers;
using Infrastructure.Events;
using Infrastructure.Extensions;
using Infrastructure.Features;
using Infrastructure.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using XmlSerializerOutputFormatter = Infrastructure.Asp.XmlSerializerOutputFormatter;

namespace Infrastructure.Startup
{
    public class DefaultStartup
    {
        private readonly Stopwatch _startupStopwatch;

        public DefaultStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            _startupStopwatch = Stopwatch.StartNew();
        }

        protected virtual void OnConfigureServices(IServiceCollection services) {}
        protected virtual void OnConfigureBeforeMvc(IApplicationBuilder app) {}
        protected virtual void OnConfigureAfterMvc(IApplicationBuilder app) {}
        protected virtual void OnConfigureRoutes(IRouteBuilder routes) {}

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            var modules = services.InstallModules(Configuration);

            services.AddMvc(options =>
                {
                    options.Filters.Add(new ResponseExceptionFilter());
                    options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
                    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                    options.ModelMetadataDetailsProviders.Add(new RequiredBindingMetadataProvider());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .RegisterModules(modules)
                .AddModulesPrefix(modules)
                .AddBaseRoute("api")
                .AddXmlSerializerFormatters()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.ConfigureApiBehavior();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(s =>
                {
                    s.WithOrigins(Configuration["Cors:AllowedOrigins"]?.Split(','));
                    s.WithHeaders(Configuration["Cors:AllowedHeaders"]?.Split(','));
                    s.WithMethods(Configuration["Cors:AllowedMethods"]?.Split(','));
                    s.DisallowCredentials();
                });
            });

            services.AddSwagger(modules);

            services.AddInfrastructure(modules);

            services.Configure<RequestResponseLoggingOptions>(Configuration);

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            OnConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment environment, ILogger<DefaultStartup> logger,
            IApplicationLifetime appLifetime)
        {
            logger.LogInformation("Starting service with environment: {0}", environment.EnvironmentName);

            IocContainer.Set(app.ApplicationServices);

            app.UseMiddleware<EnableRewindMiddleware>();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseMiddleware<RewriteContentTypeMiddleware>();

            appLifetime.ApplicationStopped.Register(() => ApplicationStopped(logger));
            appLifetime.ApplicationStarted.Register(() =>
            {
                ReadyController.ApplicationIsReady();
                _startupStopwatch.Stop();
                logger.LogInformation("Application started in {0} ms", _startupStopwatch.ElapsedMilliseconds);
            });

            app.UseSwagger();
            app.UseFeatures();
            OnConfigureBeforeMvc(app);
            app.UseMvc(OnConfigureRoutes);
            OnConfigureAfterMvc(app);

            app.StartEventProcessors();
            InitService.InitServices(app.ApplicationServices);
        }

        private void ApplicationStopped(ILogger logger)
        {
            logger.LogInformation("Application stopped");
            Log.CloseAndFlush();
        }
    }
}
