using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Infrastructure.Asp
{
    public class EnableRewindMiddleware
    {
        private readonly RequestDelegate _next;

        public EnableRewindMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Request.EnableRewind();
            return _next.Invoke(context);
        }
    }
}
