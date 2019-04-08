namespace Infrastructure.Exceptions
{
    public class TableUpdateException : RepositoryException
    {
        public TableUpdateException(string message) : base(message)
        {
        }
    }
}