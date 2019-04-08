using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Asp
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var options = context.RequestServices.GetService<IOptionsSnapshot<RequestResponseLoggingOptions>>().Value;
            
            if (!options.RequestResponseDebug)
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.Value.Contains("/docs/") ||
                options.RequestResponseDebugIgnore?.Any(x => context.Request.Path.Value.Contains(x, StringComparison.OrdinalIgnoreCase)) == true ||
                context.Request.Method.ToUpperInvariant() == "OPTIONS")
            {
                await _next(context);
                return;
            }

            try
            {
                var serializedRequest = await SerializeRequestAsync(context.Request);
                _logger.LogInformation(
                    $"Got request from {context.Connection?.RemoteIpAddress}: \n{serializedRequest}");
            }
            catch (TaskCanceledException)
            {
                // Request was canceled
                _logger.LogInformation("Request was canceled");
                return;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Error logging request");
            }

            using (var loggableResponseStream = new MemoryStream())
            {
                var originalResponseStream = context.Response.Body;
                context.Response.Body = loggableResponseStream;

                try
                {
                    await _next(context);

                    loggableResponseStream.Position = 0;
                    var serializedResponse = await SerializeResponseAsync(context.Response);
                    _logger.LogInformation($"Built response: \n{serializedResponse}");
                    loggableResponseStream.Position = 0;
                    if (context.Response.StatusCode != StatusCodes.Status204NoContent)
                        await loggableResponseStream.CopyToAsync(originalResponseStream);
                }
                finally
                {
                    context.Response.Body = originalResponseStream;
                }
            }
        }

        private async Task<string> SerializeRequestAsync(HttpRequest request)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{request.Method.ToUpperInvariant()} {request.Path.Value}{request.QueryString}");
            AppendHeaderDictionary(builder, request.Headers);

            using (var reader = CreateReader(request.Body))
            {
                var bodyString = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(bodyString))
                {
                    builder.AppendLine();
                    builder.AppendLine(bodyString);
                }
            }

            request.Body.Position = 0;

            return builder.ToString();
        }

        private async Task<string> SerializeResponseAsync(HttpResponse response)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"HTTP/1.1 {response.StatusCode}");
            AppendHeaderDictionary(builder, response.Headers);

            using (var reader = CreateReader(response.Body))
            {
                var bodyString = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(bodyString))
                    return builder.ToString();

                builder.AppendLine();
                builder.AppendLine(bodyString);
            }

            return builder.ToString();
        }

        private static void AppendHeaderDictionary(StringBuilder builder, IHeaderDictionary headers)
        {
            foreach (var header in headers) builder.AppendLine($"{header.Key}: {header.Value}");
        }

        private static StreamReader CreateReader(Stream stream)
        {
            return new StreamReader(stream, Encoding.UTF8, true, 1024, true);
        }
    }

    public class RequestResponseLoggingOptions
    {
        public bool RequestResponseDebug { get; set; }
        public string[] RequestResponseDebugIgnore { get; set; }
    }
}