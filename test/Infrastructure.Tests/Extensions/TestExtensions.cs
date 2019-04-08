using System;
using Infrastructure.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Infrastructure.Tests.Extensions
{
    public static class TestExtensions
    {
        public static Logger_Mock<T> AddLoggerMock<T>(this IServiceCollection services)
        {
            var container = new Logger_Mock<T>();
            services.AddScoped(_ => container.Get());
            return container;
        }

        public static void CheckNewDateTime(this DateTime dt)
        {
            dt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
        }
    }
}