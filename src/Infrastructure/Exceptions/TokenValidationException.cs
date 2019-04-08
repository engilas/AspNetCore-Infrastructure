using System;

namespace Infrastructure.Exceptions
{
    public class TokenValidationException : Exception
    {
        public TokenValidationException(string message) : base(message)
        {
        }

        public TokenValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}