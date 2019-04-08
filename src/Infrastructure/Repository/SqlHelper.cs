using System;
using System.Threading.Tasks;
using Infrastructure.Exceptions;
using MySql.Data.MySqlClient;

namespace Infrastructure.Repository
{
    public static class SqlHelper
    {
        /// <summary>
        /// Wrap <see cref="MySqlException"/> into <see cref="RepositoryException"/> for known error codes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <exception cref="RepositoryException"></exception>
        /// <returns></returns>
        public static async Task<T> WrapSqlException<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new RepositoryException(RepositoryError.DUPLICATE_ENTRY, ex);
            }
            catch (MySqlException ex) when(ex.Number == 1452)
            {
                throw new RepositoryException(RepositoryError.INSERT_FK_FAIL, ex);
            }
        }

        /// <summary>
        /// Wrap <see cref="MySqlException"/> into <see cref="RepositoryException"/> for known error codes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <exception cref="RepositoryException"></exception>
        /// <returns></returns>
        public static T WrapSqlException<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new RepositoryException(RepositoryError.DUPLICATE_ENTRY, ex);
            }
            catch (MySqlException ex) when(ex.Number == 1452)
            {
                throw new RepositoryException(RepositoryError.INSERT_FK_FAIL, ex);
            }
        }

        /// <summary>
        /// Wrap <see cref="MySqlException"/> into <see cref="RepositoryException"/> for known error codes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <exception cref="RepositoryException"></exception>
        /// <returns></returns>
        public static async Task WrapSqlException(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new RepositoryException(RepositoryError.DUPLICATE_ENTRY, ex);
            }
            catch (MySqlException ex) when(ex.Number == 1452)
            {
                throw new RepositoryException(RepositoryError.INSERT_FK_FAIL, ex);
            }
        }

        /// <summary>
        /// Wrap <see cref="MySqlException"/> into <see cref="RepositoryException"/> for known error codes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <exception cref="RepositoryException"></exception>
        /// <returns></returns>
        public static void WrapSqlException(Action action)
        {
            try
            {
                action();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                throw new RepositoryException(RepositoryError.DUPLICATE_ENTRY, ex);
            }
            catch (MySqlException ex) when(ex.Number == 1452)
            {
                throw new RepositoryException(RepositoryError.INSERT_FK_FAIL, ex);
            }
        }

        public static void CheckUpdateAffected(int rows, string tableName, int id)
        {
            if (rows != 1) throw new RepositoryException(RepositoryError.UPDATE_NOT_AFFECTED, $"Id {id}. Update table {tableName} failed");
        }

        public static void CheckUpdateAffected(int rows, int count, string tableName, int id)
        {
            if (rows != count)
                throw new RepositoryException(RepositoryError.UPDATE_NOT_AFFECTED,
                    $"Id {id}. Update table {tableName} failed, updated {rows} rows, expected {count}");
        }

        public static void CheckUpdateAffected(int rows, string tableName, string message)
        {
            if (rows != 1) throw new RepositoryException(RepositoryError.UPDATE_NOT_AFFECTED, $"{message}. Update table {tableName} failed");
        }

        public static void CheckUpdateAffected(int rows, int count, string tableName, string message)
        {
            if (rows != count)
                throw new RepositoryException(RepositoryError.UPDATE_NOT_AFFECTED,
                    $"{message}. Update table {tableName} failed, updated {rows} rows, expected {count}");
        }
    }
}
