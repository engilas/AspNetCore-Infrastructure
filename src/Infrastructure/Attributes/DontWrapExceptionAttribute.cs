using System;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Attributes
{
    /// <summary>
    ///     Attribute which say exception filter to not wrap exceptions and return http 500 error code
    /// </summary>
    public class DontWrapExceptionAttribute : Attribute, IExceptionFilter
    {
        public int StatusCode { get; set; } = 500;

        public void OnException(ExceptionContext context)
        {
            var logger =
                LogFactory.GetLogger(
                    context.ActionDescriptor.CastTo<ControllerActionDescriptor>().ControllerTypeInfo);

            logger.LogError(context.Exception,
                "Exception occured in controller method {0}", context.ActionDescriptor.DisplayName);

            context.Result = new ObjectResult("Internal error occured") {StatusCode = StatusCode};
            context.ExceptionHandled = true;
        }
    }
}