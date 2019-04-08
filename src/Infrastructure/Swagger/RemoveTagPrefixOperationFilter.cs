using Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Infrastructure.Swagger
{
    public class RemoveTagPrefixOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.OperationId =
                context.ApiDescription.ActionDescriptor.CastTo<ControllerActionDescriptor>().ActionName;
        }
    }
}