using System;
using Infrastructure.Response;

namespace Infrastructure.Exceptions
{
    public class ErrorException : ResponseException
    {
        public ErrorException(string message) : this(message, null)
        {
        }

        public ErrorException(string message, Exception innerException, bool logException = true) :
            base(ResponseCode.ERROR, message, innerException, logException)
        {
        }
    }
}