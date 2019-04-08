using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Configuration
{
    public static class AppConfigurations
    {
        public static IConfigurationRoot Get(string path, string[] args = null, string environmentName = null,
            string configName = "appsettings", bool substituteVariables = false)
        {
            var config = BuildConfiguration(path, args, environmentName, configName).Build();
            if (substituteVariables) SubstituteVariables(config);
            return config;
        }

        //public static IConfigurationRoot GetNoCached(string path, string[] args = null, string environmentName = null,
        //    string configName = "appsettings", bool substituteVariables = false)
        //{
        //    var config = BuildConfiguration(path, args, environmentName, configName);
        //    if (substituteVariables) SubstituteVariables(config);
        //    return config;
        //}

        public static IConfigurationBuilder BuildConfiguration(string path, string[] args,
            string environmentName = null, string configName = "appsettings", IConfigurationBuilder builder = null)
        {
            if (builder == null)
                builder = new ConfigurationBuilder();

            builder.SetBasePath(path)
                .AddJsonFile($"{configName}.json", false, true);

            if (!string.IsNullOrWhiteSpace(environmentName))
                builder = builder.AddJsonFile($"{configName}.{environmentName}.json", false,
                    true);

            builder.AddEnvironmentVariables();

            if (args != null) builder.AddCommandLine(args);

            return builder;
        }

        public static void SubstituteVariables(IConfiguration configuration)
        {
            var variables = configuration.GetSection("variables").Get<Dictionary<string, string>>();
            if (variables?.Any() != true) return;
            var keys = variables.Keys;

            foreach (var smth in configuration.AsEnumerable(true))
            foreach (var varName in keys)
                if (smth.Value?.Equals($"${varName}$") == true)
                    configuration[smth.Key] = variables[varName];
        }
    }
}