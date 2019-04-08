using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Background
{
    internal interface IBackgroundTaskManager
    {
        Task<(string, Func<CancellationToken, Task>)> DequeueAsync(
            CancellationToken cancellationToken, TaskQueueLevel level);
    }
}