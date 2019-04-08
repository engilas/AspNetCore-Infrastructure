using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Modules
{
    public class ModulesInstaller
    {
        private readonly Dictionary<Type, bool> _initializedModules = new Dictionary<Type, bool>();
        private readonly ILogger _logger = LogFactory.GetLogger<ModulesInstaller>();

        public ModuleCollection Install(IServiceCollection services, IConfiguration configuration)
        {
            services.ThrowIfNullArgument(nameof(services));
            configuration.ThrowIfNullArgument(nameof(configuration));
            var modules = CreateModules(services, configuration);
            return new ModuleCollection(modules);
        }

        private IEnumerable<ModuleDescription> CreateModules(IServiceCollection services, IConfiguration configuration)
        {
            var list = new List<ModuleDescription>();
            foreach (var section in configuration.GetSection("modules").GetChildren())
            {
                var moduleDescription = section.Get<ModuleDescription>();
                if (!moduleDescription.Enabled) continue;
                var created = CreateModule(services, moduleDescription, configuration);
                if (created) list.Add(moduleDescription);
            }

            return list;
        }

        private bool CreateModule(IServiceCollection services, ModuleDescription description,
            IConfiguration configuration)
        {
            var asm = LoadAssembly(description.AssemblyFile);
            if (asm == null)
            {
                _logger.LogError("Loading module '{0}' failed", description.Name);
                return false;
            }

            var module = GetModule(asm);
            if (module == null)
            {
                _logger.LogError("Can't find module class in assembly {0}", asm.FullName);
                return false;
            }

            description.ModuleAssembly = asm;
            description.FeaturesConfiguration = configuration.GetSection($"modules:{description.Name}:features");

            try
            {
                InitializeModule(module, services, configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed on module initialization '{0}'", description.Name);
                return false;
            }

            _logger.LogInformation("Module '{0}' loaded", description.Name);
            return true;
        }

        private void InitializeModule(IAspModule module, IServiceCollection services,
            IConfiguration configuration)
        {
            var moduleType = module.GetType();
            if (_initializedModules.TryGetValue(moduleType, out var status))
            {
                if (!status)
                {
                    throw new Exception($"Module {moduleType.Name} already failed on initialize");
                }
                return;
            }
            _initializedModules.Add(moduleType, false);


            //recurse init dependency modules
            var submoduleTypes = moduleType.GetCustomAttribute<DependsOnAttribute>()?.ModuleTypes;
            if (submoduleTypes?.Any() == true)
                foreach (var submoduleType in submoduleTypes)
                {
                    var submodule = Activator.CreateInstance(submoduleType) as IAspModule;
                    InitializeModule(submodule, services, configuration);
                }

            module.Init(services, configuration);
            _initializedModules[moduleType] = true;
        }

        private Assembly LoadAssembly(string asmFile)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), asmFile);
            Assembly asm = null;
            try
            {
                asm = Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot load assembly from path '{0}'", asmFile);
                return null;
            }

            return asm;
        }

        private IAspModule GetModule(Assembly assembly)
        {
            var installerType = assembly.GetTypes().FirstOrDefault(x => typeof(IAspModule).IsAssignableFrom(x));
            if (installerType != null) return Activator.CreateInstance(installerType) as IAspModule;
            return null;
        }
    }
}