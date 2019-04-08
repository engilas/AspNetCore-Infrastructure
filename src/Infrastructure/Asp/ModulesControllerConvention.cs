using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Infrastructure.Asp
{
    /// <summary>
    ///     Add prefixes and groups for modules
    /// </summary>
    public class ModulesControllerConvention : IControllerModelConvention
    {
        private readonly Dictionary<string, (string Route, string Name)> _assemblyPrefixDics;

        public ModulesControllerConvention(Dictionary<string, (string Route, string Name)> assemblyPrefixDics)
        {
            _assemblyPrefixDics = assemblyPrefixDics;
        }

        public void Apply(ControllerModel controller)
        {
            if (_assemblyPrefixDics.TryGetValue(Path.GetFileName(controller.ControllerType.Assembly.Location),
                out var info))
            {
                AddPrefixes(controller, info.Route);
                controller.ApiExplorer.GroupName = info.Name;
            }
        }

        private void AddPrefixes(ControllerModel controller, string prefix)
        {
            foreach (var selectorModel in controller.Selectors.ToList())
            {
                // Merge existing route models with the api prefix
                var originalAttributeRoute = selectorModel.AttributeRouteModel;
                var prefixRoute = new AttributeRouteModel(new RouteAttribute(prefix));
                //if controller does not have route, using base route
                if (originalAttributeRoute == null)
                    selectorModel.AttributeRouteModel = prefixRoute;
                else
                    selectorModel.AttributeRouteModel =
                        AttributeRouteModel.CombineAttributeRouteModel(prefixRoute, originalAttributeRoute);
            }
        }
    }
}