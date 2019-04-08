using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dapper;
using Infrastructure.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Base
{
    public class TestBase
    {
        public const int UnreachableId = (int) 1e5;

        private static readonly HashSet<string> DbInits = new HashSet<string>();
        private static readonly object SyncObj = new object();
        public IServiceProvider ServiceProvider { get; protected set; }
        public IConfiguration Configuration { get; protected set; }
        public string ConnStr { get; protected set; }

        public int GetUniqueId()
        {
            return Helpers.GetUniqueId();
        }

        public virtual void UsingDb(Action<MySqlConnection> action)
        {
            Helpers.UsingDb(ConnStr, action);
        }

        public virtual T UsingDb<T>(Func<MySqlConnection, T> func)
        {
            return Helpers.UsingDb(ConnStr, func);
        }

        public virtual void UsingTransaction(Action<MySqlTransaction> action)
        {
            Helpers.UsingDb(ConnStr, connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    action(transaction);
                    transaction.Commit();
                }
            });
        }

        public virtual T UsingTransaction<T>(Func<MySqlTransaction, T> func)
        {
            return Helpers.UsingDb(ConnStr, connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var result = func(transaction);
                    transaction.Commit();
                    return result;
                }
            });
        }

        protected IConfiguration GetConfigForModule(string module)
        {
            return Configuration.GetSection($"modules:{module}:configuration");
        }

        protected void InitializeDbWithConnStr(string connStr, string dbFile, string dbTag = null)
        {
            ConnStr = connStr;
            InitDb(dbFile, dbTag);
        }

        protected void InitializeModuleDb(string module, string dbFile = null, string dbTag = null)
        {
            ConnStr = Configuration.GetSection($"modules:{module}:configuration").GetConnectionString("default");
            InitDb(dbFile, dbTag);
        }

        protected void InitializeDb(string connectionString, string dbFile = null, string dbTag = null)
        {
            ConnStr = connectionString;
            InitDb(dbFile, dbTag);
            if (ServiceProvider != null)
                InitService.InitServices(ServiceProvider);
        }

        private void InitDb(string dbFile = null, string dbTag = null)
        {
            lock (SyncObj)
            {
                var connectionStringBuilder = new MySqlConnectionStringBuilder(ConnStr);
                var dbName = connectionStringBuilder.Database;
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    throw new Exception("Can't get db name from conn string");
                }
                if (!string.IsNullOrWhiteSpace(dbTag))
                {
                    dbName += $"_{dbTag}";
                    connectionStringBuilder.Database = dbName;
                    ConnStr = connectionStringBuilder.GetConnectionString(true);
                }

                if (DbInits.Contains(dbName)) return;
                DbInits.Add(dbName);
                connectionStringBuilder.Database = null;
                using (var connection = new MySqlConnection(connectionStringBuilder.ToString()))
                {
                    connection.Open();
                    connection.Execute($"DROP DATABASE IF EXISTS {dbName};");
                    connection.Execute($"CREATE DATABASE {dbName};");
                    connection.Execute($"USE {dbName};");

                    if (!string.IsNullOrWhiteSpace(dbFile))
                    {
                        var coreScript = File.OpenText(dbFile).ReadToEnd();
                        var script = new MySqlScript(connection, coreScript);
                        script.Execute();
                    }
                }
            }
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        protected void PeriodicalCheckResult(Action assertAction, int maxCount = 10, int pause = 100)
        {
            for (int i = 0; i < maxCount; i++)
            {
                try
                {
                    assertAction();
                    return;
                }
                catch (ShouldAssertException)
                {
                    Thread.Sleep(pause);
                }
            }
            Assert.True(false, "Periodical check failed");
        }
    }
}