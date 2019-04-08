using Infrastructure.Response;

namespace Infrastructure.Exceptions
{
    public class NotFoundException : ResponseException
    {
        public NotFoundException() : this("")
        {
        }

        public NotFoundException(string message, bool logException = true) : base(ResponseCode.NOT_FOUND, message, null,
            logException)
        {
        }
    }
}