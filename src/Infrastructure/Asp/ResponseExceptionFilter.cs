using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Asp
{
    /// <summary>
    ///     Handles exceptions from controllers
    /// </summary>
    public class ResponseExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var logger =
                LogFactory.GetLogger(
                    context.ActionDescriptor.CastTo<ControllerActionDescriptor>().ControllerTypeInfo);

            string message;
            ResponseCode code;
            var logException = true;

            switch (context.Exception)
            {
                case ResponseException ex:
                {
                    message = ex.Message;
                    code = ex.Code;
                    logException = ex.LogException;
                    break;
                }
                case TokenValidationException _:
                {
                    message = "Token validation failed";
                    code = ResponseCode.ERROR;
                    break;
                }
                default:
                {
                    message = "Internal error";
                    code = ResponseCode.ERROR;
                    break;
                }
            }

            if (logException) logger.LogError(context.Exception, "Exception occurred in controller");

            context.Result = new BaseResponse
            {
                Message = message,
                Status = code
            }.ToActionResult();

            context.ExceptionHandled = true;
        }
    }
}