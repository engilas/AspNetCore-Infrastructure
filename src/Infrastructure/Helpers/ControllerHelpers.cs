using Infrastructure.Extensions;
using Infrastructure.Logging;
using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Helpers
{
    public class ControllerHelpers
    {
        private static readonly ILogger Logger = LogFactory.GetLogger<ControllerHelpers>();

        public static BaseResponse BadRequest(ModelStateDictionary modelState)
        {
            var errors = modelState.ToDictionary();
            var errorList = modelState.ConvertToString();
            Logger.LogError("Validation errors: {0}", errors);
            return new BaseResponse {Message = errorList, Status = ResponseCode.ERROR};
        }
    }
}