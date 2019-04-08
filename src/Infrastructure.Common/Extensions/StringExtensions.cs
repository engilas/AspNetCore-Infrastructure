using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string source, string value)
        {
            return string.Equals(source, value, StringComparison.OrdinalIgnoreCase);
        }

        public static string ThrowIfNullOrWhitespace(this string source, string paramName)
        {
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));
            return !string.IsNullOrWhiteSpace(source)
                ? source
                : throw new ArgumentNullException(paramName, $"Parameter {paramName} is null or whitespace");
        }

        public static string ReplaceIfNullOrWhitespace(this string source, string replaceWith)
        {
            return string.IsNullOrWhiteSpace(source) ? replaceWith : source;
        }

        public static int ToInt32(this string source)
        {
            return int.Parse(source, NumberStyles.Any);
        }

        public static bool TryToInt32(this string source, out int result)
        {
            return int.TryParse(source, out result);
        }

        public static long ToInt64(this string source)
        {
            return long.Parse(source, NumberStyles.Any);
        }

        public static string GetMd5Hash(this string input)
        {
            var provider = new MD5CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(input);
            bytes = provider.ComputeHash(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static string GetSha1Hash(this string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static T FromJson<T>(this string json)
        {
            json.ThrowIfNullOrWhitespace(nameof(json));
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T FromJson<T>(this string json, T anonymousObject)
        {
            json.ThrowIfNullOrWhitespace(nameof(json));
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static bool In(this string source, StringComparison comparison, params string[] values)
        {
            foreach (var value in values)
            {
                if (value.Equals(source, comparison))
                    return true;
            }

            return false;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool IsEmpty(this string source) => string.IsNullOrWhiteSpace(source);
    }
}