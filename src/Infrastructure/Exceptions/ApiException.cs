using System;
using System.Net;
using System.Net.Http;

namespace Infrastructure.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(HttpStatusCode code, string reason, string response)
            : base($"Code {(int) code}: {reason}. {response}")
        {
            Code = code;
            Reason = reason;
            Response = response;
        }

        public ApiException(string message, HttpRequestException inner) : base(message, inner)
        {
        }

        public ApiException(HttpRequestException inner) : base("Http transport exception", inner)
        {
        }

        public HttpStatusCode? Code { get; }
        public string Reason { get; }
        public string Response { get; }
    }
}