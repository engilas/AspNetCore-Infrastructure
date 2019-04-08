using System;
using System.Threading;

namespace Infrastructure.Asp
{
    public static class IocContainer
    {
        private static volatile int _init;
        private static IServiceProvider _container;
        private static readonly object Sync = new object();

        public static void Set(IServiceProvider container)
        {
            lock (Sync)
            {
                if (Interlocked.CompareExchange(ref _init, 1, 0) == 0)
                    _container = container;
            }
        }

        public static IServiceProvider Get()
        {
            if (_container != null) return _container;
            lock (Sync)
            {
                if (_init != 1)
                    throw new InvalidOperationException("Container hasn't set");
                return _container;
            }
        }
    }
}
