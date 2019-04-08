using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Infrastructure.Repository
{
    public interface IDbConnectionFactory
    {
        Task UsingConnection<T>(DbOptions<T> options, Func<MySqlConnection, Task> action);
        Task<T> UsingConnection<T, TOpts>(DbOptions<TOpts> options, Func<MySqlConnection, Task<T>> func);

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <param name="existedConnection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task UsingConnection(Func<MySqlConnection, Task> action, MySqlConnection existedConnection = null);

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <param name="existedConnection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        void UsingConnection(Action<MySqlConnection> action, MySqlConnection existedConnection = null);

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedConnection"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        Task<T> UsingConnection<T>(Func<MySqlConnection, Task<T>> func, MySqlConnection existedConnection = null);

        /// <summary>
        ///     Uses existed connection or creates new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedConnection"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        T UsingConnection<T>(Func<MySqlConnection, T> func, MySqlConnection existedConnection = null);

        /// <summary>
        ///     Uses existed transaction or creates new connection with (will commit) or without transaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedTransaction"></param>
        /// <param name="action"></param>
        /// <param name="createTransactionIfNull"></param>
        /// <returns></returns>
        Task UsingTransaction(Func<MySqlConnection, MySqlTransaction, Task> action, MySqlTransaction existedTransaction = null, bool createTransactionIfNull = true);

        /// <summary>
        ///     Uses existed transaction or creates new connection with (will commit) or without transaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existedTransaction"></param>
        /// <param name="func"></param>
        /// <param name="createTransactionIfNull"></param>
        /// <returns></returns>
        Task<T> UsingTransaction<T>(Func<MySqlConnection, MySqlTransaction, Task<T>> func, MySqlTransaction existedTransaction = null, bool createTransactionIfNull = true);
    }
}