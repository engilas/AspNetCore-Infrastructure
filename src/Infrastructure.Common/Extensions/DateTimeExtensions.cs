using System;

namespace Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static long UtcToUnixTimeSeconds(this DateTime utcDateTime)
        {
            var dt = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
            return dt.ToUnixTimeSeconds();
        }

        public static long UtcToUnixTimeMilliseconds(this DateTime utcDateTime)
        {
            var dt = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
            return dt.ToUnixTimeMilliseconds();
        }
    }
}