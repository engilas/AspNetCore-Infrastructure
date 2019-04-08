using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Extensions
{
    public static class BaseResponseExtensions
    {
        public static ActionResult ToActionResult(this BaseResponse response)
        {
            return new OkObjectResult(response);
        }
    }
}
