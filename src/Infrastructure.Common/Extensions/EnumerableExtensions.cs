using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class EnumerableExtensions
    {
        public static Task AsyncForeach<T>(this IEnumerable<T> sequence, Func<T, Task> func)
        {
            if (!sequence.HasElements()) return Task.CompletedTask;
            return Task.WhenAll(sequence.Select(func));
        }

        public static Task<TResult[]> AsyncForeach<T, TResult>(this IEnumerable<T> sequence, Func<T, Task<TResult>> func)
        {
            if (!sequence.HasElements()) throw new ArgumentException("Sequence is empty");
            return Task.WhenAll(sequence.Select(func));
        }
    }
}
