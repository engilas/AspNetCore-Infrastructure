using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infrastructure.Modules
{
    public class ModuleCollection
    {
        internal ModuleCollection(IEnumerable<ModuleDescription> modules)
        {
            Modules = modules;
        }

        internal IEnumerable<ModuleDescription> Modules { get; }
        internal Assembly[] GetModulesAssemblies => Modules.Select(x => x.ModuleAssembly).ToArray();
    }
}