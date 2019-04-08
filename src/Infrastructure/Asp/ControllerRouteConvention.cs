using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Infrastructure.Asp
{
    /// <summary>
    ///     Set base route for controllers if not exists
    /// </summary>
    public class ControllerRouteConvention : IControllerModelConvention
    {
        private readonly AttributeRouteModel _baseRoute;

        public ControllerRouteConvention(string baseRoute)
        {
            _baseRoute = new AttributeRouteModel(new RouteAttribute(baseRoute));
        }

        public void Apply(ControllerModel controller)
        {
            foreach (var selectorModel in controller.Selectors.ToList())
            {
                // Merge existing route models with the api prefix
                var originalAttributeRoute = selectorModel.AttributeRouteModel;
                //if controller does not have route, using base route
                if (originalAttributeRoute == null)
                    selectorModel.AttributeRouteModel = _baseRoute;
                else
                    selectorModel.AttributeRouteModel =
                        AttributeRouteModel.CombineAttributeRouteModel(_baseRoute, originalAttributeRoute);
            }
        }
    }
}