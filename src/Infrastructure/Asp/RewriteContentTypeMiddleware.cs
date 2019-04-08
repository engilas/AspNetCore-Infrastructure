using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Asp
{
    public class RewriteContentTypeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RewriteContentTypeMiddleware> _logger;
        private readonly RewriteOptions[] _rewriteOptions;

        public RewriteContentTypeMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<RewriteContentTypeMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            _rewriteOptions = configuration.GetSection("contentRewrite").Get<RewriteOptions[]>();
        }

        public Task Invoke(HttpContext context)
        {
            if (!_rewriteOptions.HasElements()) return _next.Invoke(context);
            var path = context.Request.Path;
            var from = context.Request.ContentType;

            if (string.IsNullOrWhiteSpace(from)) return _next.Invoke(context);
            
            foreach (var options in _rewriteOptions)
            {
                if (options.CompiledRegex.IsMatch(path) && from.Contains(options.From, StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.ContentType = options.To;
                    _logger.LogDebug($"Content type header set from {from} to {options.To} for route {path}");
                }
            }

            return _next.Invoke(context);
        }
    }

    public class RewriteOptions
    {
        private Regex _compiledRegex;
        public string From { get; set; }
        public string To { get; set; }
        public string Regex { get; set; }

        public Regex CompiledRegex => _compiledRegex ?? (_compiledRegex = !string.IsNullOrWhiteSpace(Regex) ? new Regex(Regex) : null);
    }
}
