namespace Infrastructure.Repository
{
    public abstract class Entity
    {
        public abstract string TableName { get; }
        public int Id { get; set; }
    }
}