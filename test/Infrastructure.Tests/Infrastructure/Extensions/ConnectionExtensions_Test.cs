using System.Collections.Generic;
using Dapper;
using Infrastructure.Exceptions;
using Infrastructure.Repository;
using Infrastructure.Tests.Base;
using Infrastructure.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure.Extensions
{
    public class ConnectionExtensions_Test : UnitTestBase
    {
        public ConnectionExtensions_Test() : base(null)
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

        private class TestEntity : Entity
        {
            public override string TableName => "test_getall";
            public string Field { get; set; }
        }

        [Fact]
        public void EnsureOpened_Test()
        {
            var conn = _baseRepository.CreateConnection();
            conn.EnsureOpened();
            conn.Close();
            Assert.Throws<RepositoryException>(() => conn.EnsureOpened());
        }

        [Fact]
        public void GetAll_Test()
        {
            UsingDb(connection =>
            {
                connection.Execute("create table test_getall (id int, field text)");
                connection.Execute("insert into test_getall(id,field) values(1, 'abc'),(2,'cde')");
            });

            var getResult = UsingDb(connection => connection.GetAll<TestEntity>()).Result;
            getResult.Count.ShouldBe(2);
            getResult[0].Id.ShouldBe(1);
            getResult[0].Field.ShouldBe("abc");
            getResult[1].Id.ShouldBe(2);
            getResult[1].Field.ShouldBe("cde");

            var getResultCondition = UsingDb(connection => connection.GetAll<TestEntity>("where id=2")).Result;
            getResultCondition.Count.ShouldBe(1);
            getResultCondition[0].Id.ShouldBe(2);

            var getResultConditionParams =
                UsingDb(connection => connection.GetAll<TestEntity>("where id=@id", new {id = 1})).Result;
            getResultConditionParams.Count.ShouldBe(1);
            getResultConditionParams[0].Id.ShouldBe(1);
        }
    }
}