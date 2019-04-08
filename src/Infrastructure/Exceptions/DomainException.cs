using System;

namespace Infrastructure.Exceptions
{
    public abstract class DomainException<T> : Exception
        where T : Enum
    {
        protected DomainException(T code, string message) : this(code, message, null)
        {
        }

        protected DomainException(T code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        public T Code { get; }

        public override string ToString()
        {
            return $"Error code: {Code}, Message: {base.ToString()}";
        }
    }
}