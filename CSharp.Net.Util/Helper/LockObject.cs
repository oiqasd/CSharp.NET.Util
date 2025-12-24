using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 采用HashCode算法，所以不能保证完全唯一，适合小范围下的获取对象锁
    /// </summary>
    public class LockObject
    {
        private static readonly Timer cleanupTimer;
        private static readonly ConcurrentDictionary<int, TimedSemaphore> _locks;
        private static readonly TimeSpan cleanupInterval = TimeSpan.FromSeconds(100);
        private static readonly TimeSpan expireTime = TimeSpan.FromSeconds(300);

        static LockObject()
        {
            _locks = new ConcurrentDictionary<int, TimedSemaphore>();
            cleanupTimer = new Timer(_ => { CleanupExpiredLocks(); }, null, expireTime, cleanupInterval);
        }

        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="key"></param>
        /// <returns>sem.Wait();sem.Release()</returns>
        public static SemaphoreSlim GetLockObj(string key)
        {
            int cdk = key.GetHashCode();
            var timedSem = _locks.GetOrAdd(cdk, _ => new TimedSemaphore());
            timedSem.Refresh();
            return timedSem.Semaphore;
        }

        /// <summary>
        /// 定时清理过期的锁对象
        /// </summary>
        /// <returns></returns>
        static void CleanupExpiredLocks()
        {
            foreach (var kvp in _locks)
            {
                if (DateTime.UtcNow - kvp.Value.LastAccessTime > expireTime)
                {
                    if (_locks.TryRemove(kvp.Key, out var removed))
                    {
                        removed.Dispose();
                    }
                }
            }

        }
        internal class TimedSemaphore : IDisposable
        {
            public DateTime LastAccessTime { get; private set; }
            public SemaphoreSlim Semaphore { get; }
            public TimedSemaphore()
            {
                Semaphore = new SemaphoreSlim(1, 1);
                LastAccessTime = DateTime.UtcNow;
            }
            public void Refresh()
                => LastAccessTime = DateTime.UtcNow;

            public void Dispose()
                => Semaphore.Dispose();
        }
    }
}
