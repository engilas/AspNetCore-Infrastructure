using System;

namespace Infrastructure.Exceptions
{
    public class InternalException : Exception
    {
        public InternalException(string message) : base(message)
        {
        }

        public InternalException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}