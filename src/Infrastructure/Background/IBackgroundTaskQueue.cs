using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Background
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(string name, Func<CancellationToken, Task> workItem, TaskQueueLevel level = TaskQueueLevel.Medium);
    }
}