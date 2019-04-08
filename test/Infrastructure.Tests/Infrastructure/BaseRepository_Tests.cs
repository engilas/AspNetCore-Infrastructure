using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Infrastructure.Exceptions;
using Infrastructure.Repository;
using Infrastructure.Tests.Base;
using Infrastructure.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class BaseRepository_Tests : UnitTestBase
    {
        public BaseRepository_Tests() : base(null)
        {
            InitializeDb(Configuration["testConnectionString"]);
            _baseRepository = new BaseRepository_Accessor(ServiceProvider.GetService<IOptions<DbOptions>>(),
                ServiceProvider.GetService<ILogger<BaseRepository_Accessor>>());
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.Configure<DbOptions>(options =>
                options.ConnectionStrings = new Dictionary<string, string> {{"default", Configuration["testConnectionString"]}});
        }

        private readonly BaseRepository_Accessor _baseRepository;

        [Fact]
        public void CheckRowsAffected_Test()
        {
            BaseRepository_Accessor.CheckUpdateAffected(1, null, null);
            var ex = Assert.Throws<RepositoryException>(() =>
                BaseRepository_Accessor.CheckUpdateAffected(0, "t123", "m123"));
            ex.Message.ShouldContain("t123");
            ex.Message.ShouldContain("m123");
            ex.Error.ShouldBe(RepositoryError.UPDATE_NOT_AFFECTED);

            ex = Assert.Throws<RepositoryException>(() => BaseRepository_Accessor.CheckUpdateAffected(2, null, null));
            ex.Error.ShouldBe(RepositoryError.UPDATE_NOT_AFFECTED);
            BaseRepository_Accessor.CheckUpdateAffected(2, 2, null, null);
            ex = Assert.Throws<RepositoryException>(() => BaseRepository_Accessor.CheckUpdateAffected(1, 2, null, null));
            ex.Error.ShouldBe(RepositoryError.UPDATE_NOT_AFFECTED);
        }

        [Fact]
        public void CreateConnection_ShouldBeOpened()
        {
            var connection = _baseRepository.CreateConnection();
            connection.EnsureOpened();
            connection.Close();
        }

        [Fact]
        public async Task CreateConnectionAsync_ShouldBeOpened()
        {
            var connection = await _baseRepository.CreateConnectionAsync();
            connection.EnsureOpened();
            await connection.CloseAsync();
        }

        [Fact]
        public async Task UsingConnection_NullConnection_ShouldBeOpened()
        {
            await _baseRepository.UsingConnection(connection =>
            {
                connection.EnsureOpened();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task UsingConnection_ExistedConnection_ShouldUseExisted()
        {
            var conn = _baseRepository.CreateConnection();

            await _baseRepository.UsingConnection(connection =>
            {
                connection.EnsureOpened();
                ReferenceEquals(conn, connection).ShouldBeTrue();
                return Task.CompletedTask;
            }, conn);
        }

        [Fact]
        public async Task UsingConnection_T_NullConnection_ShouldBeOpened()
        {
            var result = await _baseRepository.UsingConnection(connection =>
            {
                connection.EnsureOpened();
                return Task.FromResult(1);
            });
        }

        [Fact]
        public async Task UsingConnection_T_ExistedConnection_ShouldUseExisted()
        {
            var conn = _baseRepository.CreateConnection();

            var result = await _baseRepository.UsingConnection(connection =>
            {
                connection.EnsureOpened();
                ReferenceEquals(conn, connection).ShouldBeTrue();
                return Task.FromResult(1);
            }, conn);
        }
        //tq

        [Fact]
        public async Task UsingTransaction_NullTransaction_ConnectionShouldBeOpened()
        {
            await _baseRepository.UsingTransaction((conn, trans) =>
            {
                conn.EnsureOpened();
                return Task.CompletedTask;
            });
        }

        [Fact]
        public async Task UsingTransaction_createTransactionIfNullFalse_TransShouldBeNull()
        {
            await _baseRepository.UsingTransaction((conn, trans) =>
            {
                conn.EnsureOpened();
                trans.ShouldBeNull();
                return Task.CompletedTask;
            }, createTransactionIfNull: false);
        }

        [Fact]
        public async Task UsingTransaction_createTransactionIfNullTrue_TransShouldNotBeNull()
        {
            await _baseRepository.UsingTransaction((conn, trans) =>
            {
                conn.EnsureOpened();
                trans.ShouldNotBeNull();
                return Task.CompletedTask;
            }, createTransactionIfNull: true);
        }

        [Fact]
        public async Task UsingTransaction_TransExists_ShouldUseExisted()
        {
            var conn = _baseRepository.CreateConnection();
            using (var trans = conn.BeginTransaction())
            {
                await _baseRepository.UsingTransaction((connection, transaction) =>
                {
                    connection.EnsureOpened();
                    ReferenceEquals(conn, transaction.Connection).ShouldBeTrue();
                    ReferenceEquals(conn, connection).ShouldBeTrue();
                    ReferenceEquals(transaction, trans).ShouldBeTrue();
                    return Task.CompletedTask;
                }, trans);
            }
        }

        [Fact]
        public async Task UsingTransaction_TransExists_ShouldNotCommit()
        {
            var conn = _baseRepository.CreateConnection();
            var tableName = "t" + Guid.NewGuid().ToString().Replace("-", "");
            conn.Execute($"create table {tableName} (id int);");
            using (var trans = conn.BeginTransaction())
            {
                await _baseRepository.UsingTransaction((connection, transaction) =>
                {
                    connection.EnsureOpened();
                    connection.Execute($"insert into {tableName} values (1)");
                    ReferenceEquals(conn, transaction.Connection).ShouldBeTrue();
                    ReferenceEquals(conn, connection).ShouldBeTrue();
                    ReferenceEquals(transaction, trans).ShouldBeTrue();
                    return Task.CompletedTask;
                }, trans);
            }
            conn.QuerySingle<int>($"select count(*) from {tableName}").ShouldBe(0);
        }

        [Fact]
        public async Task UsingTransaction_TransNull_ShouldCommit()
        {
            var conn = _baseRepository.CreateConnection();
            var tableName = "t" + Guid.NewGuid().ToString().Replace("-", "");
            conn.Execute($"create table {tableName} (id int);");

            MySqlTransaction trans = null;
            await _baseRepository.UsingTransaction((connection, transaction) =>
            {
                connection.Execute($"insert into {tableName} values (1)");
                connection.EnsureOpened();
                transaction.ShouldNotBeNull();
                trans = transaction;
                return Task.CompletedTask;
            }, createTransactionIfNull: true);
            trans.Connection.State.ShouldBe(ConnectionState.Closed);

            conn.QuerySingle<int>($"select count(*) from {tableName}").ShouldBe(1);
        }
        //tt
        [Fact]
        public async Task UsingTransaction_T_NullTransaction_ConnectionShouldBeOpened()
        {
            await _baseRepository.UsingTransaction((conn, trans) =>
            {
                conn.EnsureOpened();
                return Task.FromResult(1);
            });
        }

        [Fact]
        public async Task UsingTransaction_T_createTransactionIfNullFalse_TransShouldBeNull()
        {
            await _baseRepository.UsingTransaction((conn, trans) =>
            {
                conn.EnsureOpened();
                trans.ShouldBeNull();
                return Task.FromResult(1);
            }, createTransactionIfNull: false);
        }

        [Fact]
        public async Task UsingTransaction_T_createTransactionIfNullTrue_TransShouldNotBeNull()
        {
            await _baseRepository.UsingTransaction((conn, trans) =>
            {
                conn.EnsureOpened();
                trans.ShouldNotBeNull();
                return Task.FromResult(1);
            }, createTransactionIfNull: true);
        }

        [Fact]
        public async Task UsingTransaction_T_TransExists_ShouldUseExisted()
        {
            var conn = _baseRepository.CreateConnection();
            using (var trans = conn.BeginTransaction())
            {
                await _baseRepository.UsingTransaction((connection, transaction) =>
                {
                    connection.EnsureOpened();
                    ReferenceEquals(conn, transaction.Connection).ShouldBeTrue();
                    ReferenceEquals(conn, connection).ShouldBeTrue();
                    ReferenceEquals(transaction, trans).ShouldBeTrue();
                    return Task.FromResult(1);
                }, trans);
            }
        }

        [Fact]
        public async Task UsingTransaction_T_TransExists_ShouldNotCommit()
        {
            var conn = _baseRepository.CreateConnection();
            var tableName = "t" + Guid.NewGuid().ToString().Replace("-", "");
            conn.Execute($"create table {tableName} (id int);");
            using (var trans = conn.BeginTransaction())
            {
                await _baseRepository.UsingTransaction((connection, transaction) =>
                {
                    connection.EnsureOpened();
                    connection.Execute($"insert into {tableName} values (1)");
                    ReferenceEquals(conn, transaction.Connection).ShouldBeTrue();
                    ReferenceEquals(conn, connection).ShouldBeTrue();
                    ReferenceEquals(transaction, trans).ShouldBeTrue();
                    return Task.FromResult(1);
                }, trans);
            }
            conn.QuerySingle<int>($"select count(*) from {tableName}").ShouldBe(0);
        }

        [Fact]
        public async Task UsingTransaction_T_TransNull_ShouldCommit()
        {
            var conn = _baseRepository.CreateConnection();
            var tableName = "t" + Guid.NewGuid().ToString().Replace("-", "");
            conn.Execute($"create table {tableName} (id int);");

            MySqlTransaction trans = null;
            await _baseRepository.UsingTransaction((connection, transaction) =>
            {
                connection.Execute($"insert into {tableName} values (1)");
                connection.EnsureOpened();
                transaction.ShouldNotBeNull();
                trans = transaction;
                return Task.FromResult(1);
            }, createTransactionIfNull: true);
            trans.Connection.State.ShouldBe(ConnectionState.Closed);

            conn.QuerySingle<int>($"select count(*) from {tableName}").ShouldBe(1);
        }
    }
}