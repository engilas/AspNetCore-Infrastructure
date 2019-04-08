using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Extensions;

namespace Infrastructure.Helpers
{
    public class HashHelper
    {
        public static string GetHmacSha512(string secret, string message)
        {
            message.ThrowIfNullOrWhitespace(nameof(message));
            return GetHmacSha512(secret, Encoding.UTF8.GetBytes(message));
        }

        public static string GetHmacSha512(string secret, byte[] message)
        {
            secret.ThrowIfNullOrWhitespace(nameof(secret));

            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var hash = GetHmacSha512(secretBytes, message);
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static byte[] GetHmacSha512(byte[] secret, byte[] message)
        {
            secret.ThrowIfNullArgument(nameof(secret));
            message.ThrowIfNullArgument(nameof(message));

            return new HMACSHA512(secret).ComputeHash(message);
        }

        public static string GetHmacSha256(string secret, string message)
        {
            message.ThrowIfNullOrWhitespace(nameof(message));
            return GetHmacSha256(secret, Encoding.UTF8.GetBytes(message));
        }

        public static string GetHmacSha256(string secret, byte[] message)
        {
            secret.ThrowIfNullOrWhitespace(nameof(secret));

            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var hash = GetHmacSha256(secretBytes, message);
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static byte[] GetHmacSha256(byte[] secret, byte[] message)
        {
            secret.ThrowIfNullArgument(nameof(secret));
            message.ThrowIfNullArgument(nameof(message));

            return new HMACSHA256(secret).ComputeHash(message);
        }

        public static bool TryGetHmacSha256(byte[] secret, ReadOnlySpan<byte> message, Span<byte> sign)
        {
            secret.ThrowIfNullArgument(nameof(secret));
            var hmac = new HMACSHA256(secret);

            return hmac.TryComputeHash(message, sign, out var written);
        }

        public static byte[] GetHmacSha1(byte[] secret, byte[] message)
        {
            secret.ThrowIfNullArgument(nameof(secret));
            message.ThrowIfNullArgument(nameof(message));

            return new HMACSHA1(secret).ComputeHash(message);
        }
    }
}