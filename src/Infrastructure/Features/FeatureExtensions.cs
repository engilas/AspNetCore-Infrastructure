using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Features
{
    public static class FeatureExtensions
    {
        public static IServiceCollection AddFeatures(this IServiceCollection services, ModuleCollection modules)
        {
            var features = new List<IFeature>();

            var featureTypes = typeof(FeatureExtensions).Assembly.GetTypes().Where(type =>
                typeof(IFeature).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass);

            foreach (var featureType in featureTypes)
            {
                var instanse = (IFeature) Activator.CreateInstance(featureType);
                instanse.Initialize(services, modules);
                features.Add(instanse);
            }

            services.AddSingleton(new FeatureCollection(features));

            return services;
        }

        public static IApplicationBuilder UseFeatures(this IApplicationBuilder app)
        {
            foreach (var feature in app.ApplicationServices.GetService<FeatureCollection>().Features)
                feature.Activate(app);
            return app;
        }
    }
}