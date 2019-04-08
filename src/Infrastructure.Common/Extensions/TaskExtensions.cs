using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class TaskExtensions
    {
#pragma warning disable 4014
        public static async Task<T[]> UnwrapToArray<T>(this Task<IEnumerable<T>> task)
        {
            task.ThrowIfNullArgument(nameof(task));
            var result = await task;
            return result.ToArray();
        }

        public static async Task<List<T>> UnwrapToList<T>(this Task<IEnumerable<T>> task)
        {
            task.ThrowIfNullArgument(nameof(task));
            var result = await task;
            return result.ToList();
        }

        public static async Task<List<T>> UnwrapToList<T>(this Task<T[]> task)
        {
            task.ThrowIfNullArgument(nameof(task));
            var result = await task;
            return result.ToList();
        }
#pragma warning restore 4014
    }
}
