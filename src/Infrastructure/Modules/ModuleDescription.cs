using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Modules
{
    internal class ModuleDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssemblyFile { get; set; }
        public string XmlDescription { get; set; }
        public bool EnableSwagger { get; set; }
        public string RoutePath { get; set; }
        public bool Enabled { get; set; }

        internal IConfiguration FeaturesConfiguration { get; set; }

        internal Assembly ModuleAssembly { get; set; }
    }
}