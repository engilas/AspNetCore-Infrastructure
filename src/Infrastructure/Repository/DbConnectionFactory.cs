using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository
{
    internal class DbConnectionFactory : BaseRepository, IDbConnectionFactory
    {
        public DbConnectionFactory(IOptions<DbOptions> dbOptions, ILogger<DbConnectionFactory> logger) : base(dbOptions, logger)
        {
        }
    }
}
