using System;
using System.IO;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Controllers
{
    [ApiController]
    [Route("[action]")]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger Logger;

        protected BaseController(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     Returns empty <see cref="BaseResponse" /> with status code 200
        /// </summary>
        /// <returns></returns>
        protected ActionResult WlOkResponse()
        {
            return new BaseResponse {Status = ResponseCode.OK}.ToActionResult();
        }

        protected ActionResult WlOkResponse<T>(T content)
        {
            return new BaseResponse<T> {Status = ResponseCode.OK, Result = content}.ToActionResult();
        }

        protected ActionResult WlMessage(string message)
        {
            return new BaseResponse {Message = message}.ToActionResult();
        }

        protected ActionResult WlError(string message = null, Exception ex = null, bool logMessage = true)
        {
            if (logMessage) Logger.LogError(ex, message);
            return new BaseResponse {Message = message, Status = ResponseCode.ERROR}.ToActionResult();
        }

        protected ActionResult WlError(string logMessage, string responseMessage, Exception ex = null)
        {
            Logger.LogError(ex, logMessage);
            return new BaseResponse {Message = responseMessage, Status = ResponseCode.ERROR}.ToActionResult();
        }

        protected ActionResult WlNotImpl(string message = null)
        {
            return new BaseResponse {Message = message, Status = ResponseCode.NOT_IMPLEMENTED}.ToActionResult();
        }

        protected ActionResult WlNotFound(string message = null)
        {
            Logger.LogError("Not found error: " + message);
            return new BaseResponse {Message = message, Status = ResponseCode.NOT_FOUND}.ToActionResult();
        }

        protected ActionResult WlBadRequest()
        {
            return ControllerHelpers.BadRequest(ModelState).ToActionResult();
        }

        protected string GetFromRoute(string name)
        {
            return (string) HttpContext.GetRouteValue(name);
        }

        protected async Task<T> GetFromBodyJson<T>()
        {
            var content = await new StreamReader(Request.Body).ReadToEndAsync();
            return content.FromJson<T>();
        }
    }
}