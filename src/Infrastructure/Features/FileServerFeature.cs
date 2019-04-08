using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Features
{
    public class FileServerFeature : IFeature
    {
        private readonly List<(ModuleDescription, FileServerFeatureOptions)> _featureOptions =
            new List<(ModuleDescription, FileServerFeatureOptions)>();

        private readonly ILogger _logger = LogFactory.GetLogger<FileServerFeature>();

        public void Initialize(IServiceCollection services, ModuleCollection moduleCollection)
        {
            var folers = new List<string>();
            var routes = new List<string>();

            foreach (var module in moduleCollection.Modules)
            {
                var section = module.FeaturesConfiguration.GetChildren()
                    .FirstOrDefault(x => x.Key.EqualsIgnoreCase("fileServer"));

                if (section == null) continue;

                var options = section.Get<FileServerFeatureOptions[]>();

                //validating
                foreach (var option in options)
                {
                    if (string.IsNullOrWhiteSpace(option.FolderPath))
                    {
                        _logger.LogError("Module {0}: empty field {1} for FileServerFeature", module.Name,
                            nameof(option.FolderPath));
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(option.RequestPath))
                    {
                        _logger.LogError("Module {0}: empty field {1} for FileServerFeature", module.Name,
                            nameof(option.RequestPath));
                        break;
                    }

                    if (folers.Any(x => x.EqualsIgnoreCase(option.FolderPath)))
                    {
                        _logger.LogError("Module {0}: folder {1} already exists", module.Name, option.FolderPath);
                        break;
                    }

                    if (routes.Any(x => x.EqualsIgnoreCase(option.RequestPath)))
                    {
                        _logger.LogError("Module {0}: route {1} already exists", module.Name, option.RequestPath);
                        break;
                    }

                    folers.Add(option.FolderPath);
                    routes.Add(option.RequestPath);

                    _featureOptions.Add((module, option));
                }
            }

            if (_featureOptions.Any()) services.AddDirectoryBrowser();
        }

        public void Activate(IApplicationBuilder app)
        {
            foreach (var (module, options) in _featureOptions)
                try
                {
                    if (options.FolderPath.Substring(0, 2).Equals("./"))
                    {
                        var path = options.FolderPath.Substring(2);
                        options.FolderPath =
                            Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), path);
                    }

                    if (!Directory.Exists(options.FolderPath))
                        Directory.CreateDirectory(options.FolderPath);

                    var requestPath = $"/{module.RoutePath}/{options.RequestPath}";

                    app.UseFileServer(new FileServerOptions
                    {
                        EnableDirectoryBrowsing = true,
                        FileProvider = new PhysicalFileProvider(options.FolderPath),
                        RequestPath = requestPath,
                        EnableDefaultFiles = true
                    });

                    _logger.LogItems("Add FileServer",
                        new {module = module.Name, folder = options.FolderPath, route = requestPath});
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Can't activate FileServerFeature for module {0}", module);
                }
        }
    }
}