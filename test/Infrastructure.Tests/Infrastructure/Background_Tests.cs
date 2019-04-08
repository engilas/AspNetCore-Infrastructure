using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Background;
using Infrastructure.Configuration;
using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Infrastructure.Tests.Infrastructure
{
    public class Background_Tests
    {
        public Background_Tests()
        {
            var root =  Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configuration = AppConfigurations.Get(root);
            LogFactory.Initialize(configuration);

            var host = new HostBuilder().ConfigureServices(services => { services.AddBackground(); })
                .Build();

            ServiceProvider = host.Services;
            _taskQueue = ServiceProvider.GetService<IBackgroundTaskQueue>();

            _hostTask = host.RunAsync();
        }

        private IServiceProvider ServiceProvider { get; }
        private readonly Task _hostTask;
        private readonly IBackgroundTaskQueue _taskQueue;

        [Fact]
        public async Task QueueWork_MultipleTimes_ShouldBeCompleted()
        {
            var signal = new SemaphoreSlim(0);
            var jobDoneCount = 0;

            for (var i = 0; i < 10; i++)
                _taskQueue.QueueBackgroundWorkItem("name", token =>
                {
                    var count = Interlocked.Increment(ref jobDoneCount);
                    if (count == 10)
                        signal.Release();
                    return Task.CompletedTask;
                });

            await signal.WaitAsync();
            jobDoneCount.ShouldBe(10);
        }

        [Fact]
        public async Task QueueWork_ShouldBeCompleted()
        {
            var signal = new SemaphoreSlim(0);
            //var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var jobDone = false;

            _taskQueue.QueueBackgroundWorkItem("name", token =>
            {
                jobDone = true;
                signal.Release();
                return Task.CompletedTask;
            });

            await signal.WaitAsync();
            jobDone.ShouldBeTrue();
        }
    }
}