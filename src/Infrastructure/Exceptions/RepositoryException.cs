using System;
using System.Linq;

namespace Infrastructure.Exceptions
{
    public class RepositoryException : Exception
    {
        public RepositoryError? Error { get; }

        public RepositoryException(string message) : this(message, null)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RepositoryException(RepositoryError error) : this(error, null, null)
        {
        }

        public RepositoryException(RepositoryError error, string message) : this(error, message, null)
        {
        }

        public RepositoryException(RepositoryError error, Exception innerException) : this(error, null, innerException)
        {
        }

        public RepositoryException(RepositoryError error, string message, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public override string ToString()
        {
            var error = Error != null ? $"Error code: {Error}" : "";
            var msg = $"Message: {base.ToString()}";
            return string.Join(", ", new[] {error, msg}.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }

    public enum RepositoryError
    {
        DUPLICATE_ENTRY,
        INSERT_FK_FAIL,
        UPDATE_NOT_AFFECTED
    }
}