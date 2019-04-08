using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background
{
    internal class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IBackgroundTaskManager _taskManager;

        public QueuedHostedService(IBackgroundTaskManager taskManager, ILogger<QueuedHostedService> logger)
        {
            _taskManager = taskManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            await new [] {TaskQueueLevel.Low, TaskQueueLevel.Medium, TaskQueueLevel.High}.AsyncForeach(async prior =>
            {
                _logger.LogInformation($"Queue #{prior} is starting");
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (LogHelper.AddRequestId())
                    {
                        var (name, workItem) = await _taskManager.DequeueAsync(cancellationToken, prior);

                        _logger.LogDebug($"Queue #{prior}: Start processing work item {name}");

                        try
                        {
                            await workItem(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                $"Queue #{prior}: Error occurred executing {nameof(workItem)}.");
                        }
                    }
                }
            });

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}