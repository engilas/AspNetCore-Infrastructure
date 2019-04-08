using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background
{
    internal class BackgroundTimedService<T> : IHostedService, IDisposable
    where T: ITimedService
    {
        private readonly ILogger<T> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _duration;
        private readonly string _serviceName = typeof(T).Name;

        private Timer _timer;
                
        public BackgroundTimedService(ILogger<T> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
            _duration = _services.GetService<T>().GetDuration();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"{_serviceName} is starting.");

                _timer = new Timer(_ =>
                {
                    using (LogHelper.AddRequestId())
                    {
                        try
                        {
                            using (var scope = _services.CreateScope())
                            {
                                _logger.LogInformation($"{_serviceName} start working.");
                                scope.ServiceProvider.GetService<T>().Action(cancellationToken).Wait(cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Unhandled exception in {_serviceName} periodical work");
                        }
                    }
                }, null, TimeSpan.Zero, _duration);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception on initialization in {_serviceName}", ex);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_serviceName} is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
