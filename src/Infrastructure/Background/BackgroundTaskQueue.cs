using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background
{
    public enum TaskQueueLevel
    {
        Low,
        Medium,
        High
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue, IBackgroundTaskManager
    {
        private readonly ILogger _logger = LogFactory.GetLogger<BackgroundTaskQueue>();

        private readonly Dictionary<TaskQueueLevel, SemaphoreSlim> _signals = new Dictionary<TaskQueueLevel, SemaphoreSlim>
            {
                {TaskQueueLevel.Low, new SemaphoreSlim(0)}, 
                {TaskQueueLevel.Medium, new SemaphoreSlim(0)}, 
                {TaskQueueLevel.High, new SemaphoreSlim(0)}
            };

        private readonly Dictionary<TaskQueueLevel, ConcurrentQueue<(string name, Func<CancellationToken, Task>)>> _queues =
            new Dictionary<TaskQueueLevel, ConcurrentQueue<(string name, Func<CancellationToken, Task>)>>()
            {
                {TaskQueueLevel.Low, new ConcurrentQueue<(string name, Func<CancellationToken, Task>)>()}, 
                {TaskQueueLevel.Medium, new ConcurrentQueue<(string name, Func<CancellationToken, Task>)>()}, 
                {TaskQueueLevel.High, new ConcurrentQueue<(string name, Func<CancellationToken, Task>)>()}
            };

        public async Task<(string, Func<CancellationToken, Task>)> DequeueAsync(
            CancellationToken cancellationToken, TaskQueueLevel level)
        {
            await _signals[level].WaitAsync(cancellationToken);
            _queues[level].TryDequeue(out var workItem);

            _logger.LogDebug("Dequeued work item {0}", workItem.name);

            return workItem;
        }

        public void QueueBackgroundWorkItem(string name,
            Func<CancellationToken, Task> workItem, TaskQueueLevel level = TaskQueueLevel.Medium)
        {
            name.ThrowIfNullOrWhitespace(nameof(name));
            workItem.ThrowIfNullArgument(nameof(workItem));
            name += $":{Guid.NewGuid()}";

            _queues[level].Enqueue((name, workItem));

            _logger.LogDebug("Queued work item {0}", name);

            _signals[level].Release();
        }
    }
}