using System;
using System.Linq;
using System.Text.RegularExpressions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Infrastructure.Logging
{
    public class LogFactory
    {
        private static SerilogLoggerProvider _loggerProvider;
        private static volatile bool _init;
        private static readonly object SyncObj = new object();

        public static void Initialize(IConfiguration configuration)
        {
            lock (SyncObj)
            {
                if (_init) return;
                //default behaviour: if route is ignored - don't log its events except errors.
                //if error message in IgnoreErrors list, don't log this error

                var ignoreList = configuration.GetSection("Serilog:ignoreList").Get<IgnoreOptions[]>()
                                     ?.Where(x => !string.IsNullOrWhiteSpace(x.Route)).ToArray() ??
                                 new IgnoreOptions[0];

                var filterLogs = false;

                if (ignoreList.Any())
                {
                    filterLogs = true;
                    foreach (var ignoreOption in ignoreList) ignoreOption.RouteRegex = new Regex(ignoreOption.Route);
                }

                Log.Logger = new LoggerConfiguration()
                    .If(filterLogs,
                        x => x.Filter.ByExcluding(log =>
                        {
                            try
                            {
                                return LogExcludeFilter(log, ignoreList.ToArray());
                            }
                            catch (Exception ex)
                            {
                                //use console to avoid stack overflow
                                Console.Error.WriteLine($"Exception in logger exclude filter: {ex.Message}");
                                return false;
                            }
                        }))
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                _loggerProvider = new SerilogLoggerProvider(Log.Logger);

                _init = true;
            }
        }

        private static bool LogExcludeFilter(LogEvent log, IgnoreOptions[] options)
        {
            if (!log.Properties.TryGetValue("RequestPath", out var routeValue))
                return false;

            var route = (routeValue as ScalarValue)?.Value as string;
            if (route == null)
                return false;

            foreach (var ignoreOption in options)
            {
                if (!ignoreOption.RouteRegex.IsMatch(route))
                    continue;

                if (log.Level > LogEventLevel.Information)
                {
                    var message = log.RenderMessage() + log.Exception?.Message;
                    foreach (var ignoredError in ignoreOption.IgnoreErrors)
                        if (message.Contains(ignoredError))
                            return true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckInit()
        {
            if (!_init)
                throw new InvalidOperationException("Logger is not initialized");
        }

        public static ILogger GetLogger<T>()
        {
            CheckInit();
            return _loggerProvider.CreateLogger(TypeNameHelper.GetTypeDisplayName(typeof(T)));
        }

        public static ILogger GetLogger(Type source)
        {
            CheckInit();
            return _loggerProvider.CreateLogger(TypeNameHelper.GetTypeDisplayName(source));
        }

        public static ILogger GetLogger(string name)
        {
            CheckInit();
            return _loggerProvider.CreateLogger(name);
        }
    }
}