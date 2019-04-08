using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace Infrastructure.Repository
{
    public static class Extensions
    {
        private static readonly ConcurrentDictionary<Type, string>
            TableNames = new ConcurrentDictionary<Type, string>();

        public static void EnsureOpened(this MySqlConnection connection)
        {
            if (connection.State != ConnectionState.Open) throw new RepositoryException("Connection is not opened");
        }

        public static async Task<List<T>> GetAll<T>(this IDbConnection connection, string sqlCondition = null,
            object parameters = null)
            where T : Entity, new()
        {
            connection.ThrowIfNullArgument(nameof(connection));
            var table = TableNames.GetOrAdd(typeof(T), _ => new T().TableName);
            return (await connection.QueryAsync<T>($"select * from {table} {sqlCondition}", parameters)).ToList();
        }

        public static async Task<T> GetById<T>(this IDbConnection connection, int id)
            where T : Entity, new()
        {
            connection.ThrowIfNullArgument(nameof(connection));
            var table = TableNames.GetOrAdd(typeof(T), _ => new T().TableName);
            return await connection.QuerySingleOrDefaultAsync<T>($"select * from {table} where id = @id", new {id});
        }

        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<DbConnectionFactory>()
                .AddScoped<IDbConnectionFactory>(provider =>
                    new SqlErrorHandlerDecorator(provider.GetService<DbConnectionFactory>()))
                .AddScoped<ISqlParamCombiner, SqlParamCombiner>();

            return services;
        }
    }
}