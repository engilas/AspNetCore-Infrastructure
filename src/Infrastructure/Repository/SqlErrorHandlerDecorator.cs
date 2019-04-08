using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Infrastructure.Repository
{
    internal class SqlErrorHandlerDecorator : IDbConnectionFactory
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public SqlErrorHandlerDecorator(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public Task UsingConnection<T>(DbOptions<T> options, Func<MySqlConnection, Task> action)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingConnection(options, action));
        }

        public Task<T> UsingConnection<T, TOpts>(DbOptions<TOpts> options, Func<MySqlConnection, Task<T>> func)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingConnection(options, func));
        }

        public Task UsingConnection(Func<MySqlConnection, Task> action, MySqlConnection existedConnection = null)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingConnection(action, existedConnection));
        }

        public void UsingConnection(Action<MySqlConnection> action, MySqlConnection existedConnection = null)
        {
            SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingConnection(action, existedConnection));
        }

        public T UsingConnection<T>(Func<MySqlConnection, T> func, MySqlConnection existedConnection = null)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingConnection(func, existedConnection));
        }

        public Task<T> UsingConnection<T>(Func<MySqlConnection, Task<T>> func, MySqlConnection existedConnection = null)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingConnection(func, existedConnection));
        }

        public Task UsingTransaction(Func<MySqlConnection, MySqlTransaction, Task> action, MySqlTransaction existedTransaction = null, bool createTransactionIfNull = false)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingTransaction(action, existedTransaction, createTransactionIfNull));
        }

        public Task<T> UsingTransaction<T>(Func<MySqlConnection, MySqlTransaction, Task<T>> func, MySqlTransaction existedTransaction = null, bool createTransactionIfNull = false)
        {
            return SqlHelper.WrapSqlException(() => _dbConnectionFactory.UsingTransaction(func, existedTransaction, createTransactionIfNull));
        }
    }
}
