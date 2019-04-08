using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Events;
using Infrastructure.Tests.Mocks.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests.Mocks
{
    class EventListener_Mock : ITestEventListener
    {
        public TestEventType ListenType { get; set; }
        public SemaphoreSlim Semaphore { get; set; }

        public Task ProcessEvent(TestEventData eventData)
        {
            Semaphore.Release();
            return Task.CompletedTask;
        }
    }

    static class EventListener_Mock_Extension
    {
        public static IServiceCollection AddEventListener_Mock<TListener>(this IServiceCollection services, SemaphoreSlim sem, TestEventType type)
        where TListener : EventListener_Mock, new ()
        {
            Func<IServiceProvider, TListener> factory = s =>
            {
                var mock = new TListener();
                mock.Semaphore = sem;
                mock.ListenType = type;
                return mock;
            };

            services.AddTransient(factory);
            services.AddTransient<IEventListener>(factory);
            

            return services;
        }
    }
}
