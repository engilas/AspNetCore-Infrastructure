using System;
using System.IO;
using System.Linq;
using AutoMapper;
using Infrastructure.Asp;
using Infrastructure.Attributes;
using Infrastructure.Configuration;
using Infrastructure.Features;
using Infrastructure.Helpers;
using Infrastructure.Logging;
using Infrastructure.Modules;
using Infrastructure.Repository;
using Infrastructure.Startup;
using Infrastructure.Utils.Async;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Extensions
{
    public static class StartupExtensions
    {
        private static readonly object SyncObj = new object();
        private static volatile bool _createdMappingsBefore;

        public static IMvcBuilder RegisterModules(this IMvcBuilder builder, ModuleCollection modules)
        {
            builder.ConfigureApplicationPartManager(manager =>
            {
                manager.ApplicationParts.Clear();
                foreach (var assembly in modules.GetModulesAssemblies)
                {
                    var part = new AssemblyPart(assembly);
                    manager.ApplicationParts.Add(part);
                }

                manager.ApplicationParts.Add(new AssemblyPart(typeof(StartupExtensions).Assembly));
            });

            return builder;
        }

        public static IMvcBuilder AddModulesPrefix(this IMvcBuilder builder, ModuleCollection modules)
        {
            //dict with assembly->route values
            var dict = modules.Modules.Select(x => new
            {
                Assembly = Path.GetFileName(x.AssemblyFile),
                Route =  x.RoutePath,
                Name = x.Name
            }).ToDictionary(arg => arg.Assembly, arg => (arg.Route, arg.Name));

            var prefixConvention = new ModulesControllerConvention(dict);
            // Insert the convention within the MVC options
            builder.Services.Configure<MvcOptions>(opts => opts.Conventions.Add(prefixConvention));

            return builder;
        }

        public static IMvcBuilder AddBaseRoute(this IMvcBuilder builder, string route)
        {
            var routeConvention = new ControllerRouteConvention(route);
            builder.Services.Configure<MvcOptions>(options => options.Conventions.Add(routeConvention));

            return builder;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection,
            ModuleCollection modules)
        {
            serviceCollection.AddAutomapper(modules)
                .AddFeatures(modules)
                .AddRepository()
                .AddScoped<IWlMemoryCache, WlMemoryCache>();

            return serviceCollection;
        }

        public static IServiceCollection AddAutomapper(this IServiceCollection serviceCollection,
            ModuleCollection modules)
        {
            lock (SyncObj)
            {
                if (!_createdMappingsBefore)
                {
                    Mapper.Initialize(config => { config.AddProfiles(modules.GetModulesAssemblies); });
                    _createdMappingsBefore = true;
                }
            }

            return serviceCollection;
        }

        public static ModuleCollection InstallModules(this IServiceCollection services, IConfiguration configuration)
        {
            var installer = new ModulesInstaller();
            var modules = installer.Install(services, configuration);
            services.AddSingleton(modules);
            return modules;
        }

        public static TOptions BuildOptions<TOptions>(this IServiceCollection services) where TOptions : class, new()
        {
            var provider = services.BuildServiceProvider();
            return provider.GetService<IOptions<TOptions>>().Value;
        }

        public static void ValidateOptions<TOptions>(this IServiceCollection services,
            Func<TOptions, string> validationFunc)
            where TOptions : class, new()
        {
            var options = services.BuildOptions<TOptions>();

            var error = validationFunc(options);
            if (!string.IsNullOrWhiteSpace(error)) throw new Exceptions.OptionsValidationException(typeof(TOptions).Name, error);
        }

        public static void ValidateOptions<TOptions>(this IServiceCollection services)
            where TOptions : OptionsBase, new()
        {
            var options = services.BuildOptions<TOptions>();
            options.Validate();
        }

        public static TOptions ConfigureAndValidate<TOptions>(this IServiceCollection services, IConfiguration configuration)
            where TOptions : OptionsBase, new()
        {
            services.Configure<TOptions>(configuration);
            var options = services.BuildOptions<TOptions>();
            options.Validate();
            return options;
        }

        public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var descriptor = context.ActionDescriptor.CastTo<ControllerActionDescriptor>();

                    if (descriptor.MethodInfo.CustomAttributes
                            .Any(x => x.AttributeType == typeof(DontWrapInvalidModelStateAttribute))
                        //todo test for controller!
                        || descriptor.ControllerTypeInfo.CustomAttributes
                            .Any(x => x.AttributeType == typeof(DontWrapInvalidModelStateAttribute)))
                    {
                        var errors = context.ModelState.ToDictionary();

                        LogFactory.GetLogger(descriptor.ControllerTypeInfo)
                            .LogError("Validation error: {Values}", errors);

                        var result = new BadRequestObjectResult(context.ModelState);
                        return result;
                    }

                    return ControllerHelpers.BadRequest(context.ModelState).ToActionResult();
                };
            });

            return services;
        }

        public static IServiceCollection AddWlMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddTransient<IWlMemoryCache, WlMemoryCache>();

            return services;
        }

        /// <summary>
        /// One singleton with two interfaces
        /// </summary>
        /// <typeparam name="TService1"></typeparam>
        /// <typeparam name="TService2"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService1, TService2, TImpl>(this IServiceCollection services)
        where TImpl : class, TService1, TService2
        where TService1: class 
        where TService2: class 
        {
            services.AddSingleton<TImpl>();
            services.AddSingleton<TService1>(s => s.GetService<TImpl>());
            services.AddSingleton<TService2>(s => s.GetService<TImpl>());
            return services;
        }

        public static IServiceCollection AddInitService<T>(this IServiceCollection services)
        where T : class, IInitService
        {
            services.AddScoped<IInitService, T>();
            return services;
        }
    }
}