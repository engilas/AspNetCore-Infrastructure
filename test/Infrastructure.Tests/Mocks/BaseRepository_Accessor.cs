using System;
using System.Threading.Tasks;
using Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Infrastructure.Tests.Mocks
{
    public class BaseRepository_Accessor : BaseRepository
    {
        public BaseRepository_Accessor(IOptions<DbOptions> dbOptions, ILogger logger) : base(dbOptions, logger)
        {
        }

        public new MySqlConnection CreateConnection()
        {
            return base.CreateConnection();
        }

        public new Task<MySqlConnection> CreateConnectionAsync()
        {
            return base.CreateConnectionAsync();
        }

        public new Task UsingConnection(Func<MySqlConnection, Task> action, MySqlConnection existedConnection = null)
        {
            return base.UsingConnection(action, existedConnection);
        }

        public new Task<T> UsingConnection<T>(Func<MySqlConnection, Task<T>> func, MySqlConnection existedConnection = null)
        {
            return base.UsingConnection(func, existedConnection);
        }

        public new static void CheckUpdateAffected(int rows, string tableName, string message)
        {
            BaseRepository.CheckUpdateAffected(rows, tableName, message);
        }

        public new static void CheckUpdateAffected(int rows, int count, string tableName, string message)
        {
            BaseRepository.CheckUpdateAffected(rows, count, tableName, message);
        }
    }
}