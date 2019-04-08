using System;
using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Exceptions
{
    public abstract class ResponseException : Exception
    {
        protected ResponseException(ResponseCode code, string message) : this(code, message, null)
        {
        }

        protected ResponseException(ResponseCode code, string message, Exception innerException,
            bool logException = true) : base(message,
            innerException)
        {
            LogException = logException;
            Code = code;
        }

        public bool LogException { get; }

        public ResponseCode Code { get; }

        public IActionResult ToActionResult()
        {
            return new ObjectResult(Message)
            {
                StatusCode = (int) Code
            };
        }
    }
}