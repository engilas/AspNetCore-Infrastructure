using System;

namespace Infrastructure.Exceptions
{
    /// <summary>
    ///     Exception for bad response from external services
    /// </summary>
    public class BadResponseException : Exception
    {
        public BadResponseException(string message) : base(message)
        {
        }
    }
}