using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class TaskHelper
    {
        public static Task WhenAll(params Func<Task>[] tasks)
        {
            return Task.WhenAll(tasks.Select(x => x()));
        }
    }
}
