using System;
using System.Linq;
using System.Reflection;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Events
{
    public static class EventExtensions
    {
        public static IApplicationBuilder StartEventProcessors(this IApplicationBuilder app)
        {
            StartEventProcessors(app.ApplicationServices);

            return app;
        }

        public static void StartEventProcessors(IServiceProvider services)
        {
            var listeners = services.GetServices(typeof(IEventListener));

            var logger = LogFactory.GetLogger("Startup");

            foreach (var listener in listeners)
            {
                var processorName = listener.GetType().Name;
                try
                {
                    var listenerType = listener.GetType();
                    var interfaceType = listenerType.GetInterfaces().Single(x => x.Name.Contains(nameof(IEventListener)) && x.IsGenericType);

                    var busType = interfaceType.GetGenericArguments()[0];
                    var eventType = interfaceType.GetGenericArguments()[1];
                    var eventDataType = interfaceType.GetGenericArguments()[2];

                    var bus = services.GetService(busType);

                    var method = typeof(EventExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains(nameof(StartListenEvent)) && x.IsGenericMethod)
                        .MakeGenericMethod(busType, eventDataType, eventType, listenerType);

                    method.Invoke(null, new[] {bus, listener});

                    logger.LogInformation("Event processor {0} started", processorName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception occurred on start event processor of type {0}", processorName);
                }
            }
        }

        private static void StartListenEvent<TBus, TEventData, TEventType, TListener>(TBus bus, TListener listener)
        where TBus : IEventBus<TEventType, TEventData>
        where TListener : IEventListener<TBus, TEventType, TEventData>
        {
            bus.SubscribeOnEvent<TListener>(listener.ListenType);
        }

        public static IServiceCollection AddEventListener<T>(this IServiceCollection services)
        where T : class, IEventListener 
        {
            services.AddTransient<IEventListener, T>();
            services.AddTransient<T>();

            return services;
        }
    }
}
