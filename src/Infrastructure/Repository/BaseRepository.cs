using System;
using System.Data.Common;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Infrastructure.Repository
{
    /// <summary>
    ///     Base class for repository implementations
    /// </summary>
    public abstract class BaseRepository
    {
        private readonly DbOptions _dbOptions;
        protected readonly ILogger Logger;

        protected BaseRepository(IOptions<DbOptions> dbOptions, ILogger logger)
        {
            Logger = logger;
            _dbOptions = dbOptions.Value;
        }

        /// <summary>
        ///     Get MySql database connetction
        /// </summary>
        /// <returns></returns>
        protected MySqlConnection CreateConnection()
        {
            var dbConn = new MySqlConnection(_dbOptions.GetConnectionString());
            dbConn.Open();
            return dbConn;
        }

        /// <summary>
        ///     Get MySql database connetction
        /// </summary>
        /// <returns></returns>
        protected async Task<MySqlConnection> CreateConnectionAsync()
        {
            var dbConn = new MySqlConnection(_dbOptions.GetConnectionString());
            await dbConn.OpenAsync();
            return dbConn;
        }

        protected async Task<MySqlConnection> CreateConnectionAsync(string connectionString)
        {
            connectionString.ThrowIfNullOrWhitespace(nameof(connectionString));
            var dbConn = new MySqlConnection(connectionString);
            await dbConn.OpenAsync();
            return dbConn;
        }

        public async Task UsingConnection<T>(DbOptions<T> options, Func<MySqlConnection, Task> action)
        {
            options.ThrowIfNullArgument(nameof(options));
            using (var connection = await CreateConnectionAsync(options.GetConnectionString()))
            {
                await action(connection);
            }
        }

        public async Task<T> UsingConnection<T, TOpts>(DbOptions<TOpts> options, Func<MySqlConnection, Task<T>> func)
        {
            options.ThrowIfNullArgument(nameof(options));
            using (var connection = await CreateConnectionAsync(options.GetConnectionString()))
            {
                return await func(connection);
            }
        }

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <param name="existedConnection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task UsingConnection(Func<MySqlConnection, Task> action, MySqlConnection existedConnection = null)
        {
            if (existedConnection != null)
                await action(existedConnection);
            else
                using (var connection = await CreateConnectionAsync())
                {
                    await action(connection);
                }
        }

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <param name="existedConnection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void UsingConnection(Action<MySqlConnection> action, MySqlConnection existedConnection = null)
        {
            if (existedConnection != null)
                action(existedConnection);
            else
                using (var connection = CreateConnection())
                {
                    action(connection);
                }
        }

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedConnection"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<T> UsingConnection<T>(Func<MySqlConnection, Task<T>> func, MySqlConnection existedConnection = null)
        {
            if (existedConnection != null)
                return await func(existedConnection);
            using (var connection = await CreateConnectionAsync())
            {
                return await func(connection);
            }
        }

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedConnection"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public T UsingConnection<T>(Func<MySqlConnection, T> func, MySqlConnection existedConnection = null)
        {
            if (existedConnection != null)
                return func(existedConnection);
            using (var connection = CreateConnection())
            {
                return func(connection);
            }
        }

        /// <summary>
        ///     Uses existed transaction or creates new connection with (will commit) or without transaction
        /// </summary>
        /// <param name="existedTransaction"></param>
        /// <param name="action"></param>
        /// <param name="createTransactionIfNull"></param>
        /// <returns></returns>
        public async Task UsingTransaction(Func<MySqlConnection, MySqlTransaction, Task> action,
            MySqlTransaction existedTransaction = null, bool createTransactionIfNull = true)
        {
            if (existedTransaction != null)
                await action(existedTransaction.Connection, existedTransaction);
            else
                using (var connection = await CreateConnectionAsync())
                {
                    if (createTransactionIfNull)
                        using (var trans = await connection.BeginTransactionAsync())
                        {
                            await action(connection, trans);
                            trans.Commit();
                        }
                    else
                        await action(connection, null);
                }
        }

        /// <summary>
        ///     Uses existed transaction or creates new connection with (will commit) or without transaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedTransaction"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<T> UsingTransaction<T>(Func<MySqlConnection, MySqlTransaction, Task<T>> func,
            MySqlTransaction existedTransaction = null, bool createTransactionIfNull = true
        )
        {
            if (existedTransaction != null)
                return await func(existedTransaction.Connection, existedTransaction);
            using (var connection = await CreateConnectionAsync())
            {
                if (createTransactionIfNull)
                    using (var trans = await connection.BeginTransactionAsync())
                    {
                        var result = await func(connection, trans);
                        trans.Commit();
                        return result;
                    }

                return await func(connection, null);
            }
        }

        /// <summary>
        ///     Get scalar SQL value
        /// </summary>
        protected async Task<string> ScalarSqlAsync(string query)
        {
            try
            {
                using (var conn = await CreateConnectionAsync())
                {
                    var cmd = new MySqlCommand(query, conn);
                    var result = await cmd.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error running scalar SQL '{0}' - {1}", query, ex.Message);
                throw;
            }
        }

        /// <summary>
        ///     Run query SQL
        /// </summary>
        protected async Task<int> RunSqlAsync(string query)
        {
            try
            {
                using (var conn = await CreateConnectionAsync())
                {
                    var cmd = new MySqlCommand(query, conn);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error running SQL '{0}' - {1}", query, ex.Message);
                throw;
            }
        }

        /// <summary>
        ///     Simplify running select queryes
        /// </summary>
        protected async Task<bool> LoadSqlAsync(string query, Func<DbDataReader, bool> run,
            MySqlTransaction trans = null)
        {
            try
            {
                if (trans == null)
                    using (var conn = await CreateConnectionAsync())
                    {
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                    if (!run(reader))
                                        return false;
                            }
                        }
                    }
                else
                    using (var cmd = new MySqlCommand(query, trans.Connection, trans))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                if (!run(reader))
                                    return false;
                        }
                    }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading SQL '{0}'", query);
                return false;
            }
        }

        /// <summary>
        ///     Simplify running no results queries
        /// </summary>
        protected async Task<int> RunSqlAsync(string query, MySqlTransaction trans)
        {
            try
            {
                using (var cmd = new MySqlCommand(query, trans.Connection, trans))
                {
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error running SQL '{0}'", query);
                return -1;
            }
        }

        protected static int GetNullableDateTime(DbDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            if (reader.IsDBNull(col)) return -1;
            return (int) ((DateTimeOffset) reader.GetDateTime(col)).ToUnixTimeSeconds();
        }

        protected static void CheckUpdateAffected(int rows, string tableName, string message)
        {
            SqlHelper.CheckUpdateAffected(rows, tableName, message);
        }

        protected static void CheckUpdateAffected(int rows, int count, string tableName, string message)
        {
            SqlHelper.CheckUpdateAffected(rows, count, tableName, message);
        }
    }
}