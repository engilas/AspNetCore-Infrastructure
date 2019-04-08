using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Background
{
    public interface ITimedService
    {
        TimeSpan GetDuration();
        Task Action(CancellationToken cancellationToken);
    }
}