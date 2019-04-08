using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Infrastructure.Extensions
{
    public static class CommonExtensions
    {
        public static T CastTo<T>(this object source)
        {
            return (T) source;
        }

        public static bool In<T>(this T source, params T[] list)
        {
            source.ThrowIfNullArgument(nameof(source));
            list.ThrowIfNullArgument(nameof(list));

            return list.Contains(source);
        }

        public static TResult Map<T, TResult>(this T source, Func<T, TResult> mapFunc)
        {
            source.ThrowIfNullArgument(nameof(source));
            mapFunc.ThrowIfNullArgument(nameof(mapFunc));
            return mapFunc(source);
        }

        public static T ThrowIfNullArgument<T>(this T source, string argumentName)
        {
            argumentName.ThrowIfNullOrWhitespace(nameof(argumentName));
            if (source == null) throw new ArgumentNullException(argumentName, $"Parameter {argumentName} is null");
            return source;
        }

        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> source, string argumentName)
        {
            argumentName.ThrowIfNullOrWhitespace(nameof(argumentName));
            if (source == null) throw new ArgumentNullException(argumentName, $"Parameter {argumentName} is null");
            if (!source.HasElements())
                throw new ArgumentException(argumentName, $"Parameter {argumentName} is empty collection");
        }

        public static T ThrowIfNull<T>(this T source, string message)
        {
            message.ThrowIfNullOrWhitespace(nameof(message));
            if (source == null) throw new NullReferenceException(message);
            return source;
        }

        public static string ToJson(this object source)
        {
            source.ThrowIfNullArgument(nameof(source));
            return JsonConvert.SerializeObject(source);
        }

        public static T If<T>(this T source, bool condition, Func<T, T> func)
        {
            if (condition)
                return func(source);
            return source;
        }

        public static bool HasElements<T>(this IEnumerable<T> collection)
        {
            return collection?.Any() == true;
        }

        public static T[] AsArray<T>(this T item) => new[] {item};
    }
}