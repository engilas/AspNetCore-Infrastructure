using System;

namespace Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static T Parse<T>(this string from) where T : Enum
        {
            from.ThrowIfNullOrWhitespace(nameof(from));
            return (T) Enum.Parse(typeof(T), from, true);
        }

        public static bool TryParse<T>(this string from, out T result) where T : struct, Enum
        {
            from.ThrowIfNullOrWhitespace(nameof(from));
            var success = Enum.TryParse(from, true, out T res);
            result = success ? res : default;
            return success;
        }

        public static T Convert<T>(this Enum from) where T : Enum
        {
            from.ThrowIfNullArgument(nameof(from));
            return Parse<T>(from.ToString());
        }
    }
}