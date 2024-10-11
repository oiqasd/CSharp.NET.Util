using System;
using System.Threading;

namespace CSharp.Net.Util
{
    public class LimitHelper
    {
        static readonly LazyConcurrentDictionary<string, LimitTask> pool = new LazyConcurrentDictionary<string, LimitTask>();

        public static LimitTask Case(string key)
        {
            return pool.GetOrAdd(key, _ => { return new LimitTask(); });
        }
        public static LimitTask Case(string key, int concurrent)
        {
            return pool.GetOrAdd(key, _ => { return new LimitTask(concurrent); });
        }

        private static readonly TimeSpan cleanupInterval = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan lockExpiryTime = TimeSpan.FromMinutes(10);
        private static readonly Timer cleanupTimer;
        static LimitHelper()
        {
            cleanupTimer = new Timer(CleanupExpiredLocks, null, cleanupInterval, cleanupInterval);
        }
        private static void CleanupExpiredLocks(object state)
        {
            DateTime now = DateTime.UtcNow;
            foreach (var kvp in pool.GetDictionary())
            {
                if (!kvp.Value.IsValueCreated)
                {
                    pool.Remove(kvp.Key);
                    continue;
                }
                if (!kvp.Value.Value.Lts && (now - kvp.Value.Value.lastReadTime) > lockExpiryTime)
                {
                    pool.Remove(kvp.Key);
                }
            }
        }
    }
}
