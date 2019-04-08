using System;
using System.Linq;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static void LogItems(this ILogger logger, string message, object items)
        {
            logger.LogInformation(GetStructuredLog(message, items));
        }

        public static void LogErrorItems(this ILogger logger, string message, object items, Exception ex = null)
        {
            logger.LogError(ex, GetStructuredLog(message, items));
        }

        public static void LogDebugItems(this ILogger logger, string message, object items, Exception ex = null)
        {
            logger.LogDebug(ex, GetStructuredLog(message, items));
        }

        public static void LogWarningItems(this ILogger logger, string message, object items, Exception ex = null)
        {
            logger.LogWarning(ex, GetStructuredLog(message, items));
        }

        private static string GetStructuredLog(string message, object items)
        {
            var props = items.GetType().GetProperties();

            return message + ". " +
                   string.Join(", ", props.Select(x => string.Format("{0}: {1}", x.Name, x.GetValue(items))));
        }

        public static void LogErrorAndThrow(this ILogger logger, string error, object logDetails, Exception ex = null)
        {
            logger.LogErrorItems(error, logDetails, ex);
            throw new ErrorException(error, null, false);
        }

        public static void LogNotFoundAndThrow(this ILogger logger, string error, object logDetails,
            Exception ex = null)
        {
            logger.LogErrorItems(error, logDetails, ex);
            throw new NotFoundException(error, false);
        }

        public static void LogErrorAndThrow(this ILogger logger, Exception ex, string error)
        {
            logger.LogError(ex, error);
            throw new ErrorException(error, null, false);
        }

        public static void LogNotFoundAndThrow(this ILogger logger, Exception ex, string error)
        {
            logger.LogError(ex, error);
            throw new NotFoundException(error, false);
        }
    }
}