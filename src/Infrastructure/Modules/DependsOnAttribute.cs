using System;

namespace Infrastructure.Modules
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(params Type[] moduleTypes)
        {
            ModuleTypes = moduleTypes;
        }

        public Type[] ModuleTypes { get; }
    }
}