using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Infrastructure.Swagger
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, ModuleCollection modules)
        {
            services.AddSwaggerGen(c =>
            {
                c.ConfigureModules(modules);
                c.DescribeAllEnumsAsStrings();
                c.OperationFilter<RemoveTagPrefixOperationFilter>();
                c.AddSecurityDefinition("Token",
                    new ApiKeyScheme
                    {
                        In = "header",
                        Name = "Authorization", 
                        Type = "apiKey",
                    });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>() {{"Token", new string[0]}});
            });
            return services;
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
        {
            var prefix = app.ApplicationServices.GetService<IConfiguration>()["swaggerPrefix"] ?? string.Empty;

            app.UseRewriter(new RewriteOptions()
                .AddRedirect("^docs/(.*)", $"{prefix}/docs/$1")
                .AddRedirect("^docs$", $"{prefix}/docs"));

            app.UseSwagger(c => { c.RouteTemplate = $"{prefix}/docs/{{documentName}}/api.json"; });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = $"{prefix}/docs";
                c.ConfigureModules($"/{prefix}/docs/{{module}}/api.json", app.ApplicationServices.GetService<ModuleCollection>());
            });
            return app;
        }

        private static void ConfigureModules(this SwaggerGenOptions c, ModuleCollection modules)
        {
            foreach (var module in modules.Modules.Where(x => x.EnableSwagger))
            {
                c.SwaggerDoc(module.Name, new Info
                {
                    Version = "v1",
                    Title = module.Name,
                    Description = module.Description
                });
                var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var xmlPath = Path.Combine(basePath, module.XmlDescription);
                c.IncludeXmlComments(xmlPath);
            }
        }

        private static void ConfigureModules(this SwaggerUIOptions c, string endPoint, ModuleCollection modules)
        {
            foreach (var module in modules.Modules.Where(x => x.EnableSwagger))
            {
                var path = endPoint.Replace("{module}", module.Name);
                c.SwaggerEndpoint(path, module.Name);
            }
        }
    }
}