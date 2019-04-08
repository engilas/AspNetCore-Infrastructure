using System;
using Serilog.Context;

namespace Infrastructure.Logging
{
    public static class LogHelper
    {
        /// <summary>
        ///     Adds a unique identifier for scope
        /// </summary>
        /// <returns></returns>
        public static IDisposable AddRequestId()
        {
            return LogContext.PushProperty("RequestId", Guid.NewGuid().ToString().Substring(0, 8));
        }
    }
}