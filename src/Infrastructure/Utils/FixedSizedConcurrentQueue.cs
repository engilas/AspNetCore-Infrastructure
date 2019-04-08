using System.Collections.Concurrent;

namespace Infrastructure.Utils
{
    public class FixedSizedConcurrentQueue<T>
    {
        private readonly object _lockObject = new object();

        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public FixedSizedConcurrentQueue(int limit)
        {
            Limit = limit;
        }

        private int Limit { get; }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);
            lock (_lockObject)
            {
                T overflow;
                while (_queue.Count > Limit && _queue.TryDequeue(out overflow)) ;
            }
        }

        public T[] GetItems()
        {
            return _queue.ToArray();
        }
    }
}