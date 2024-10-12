using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 采用HashCode算法，所以不能保证完全唯一，适合小范围下的获取对象锁
    /// </summary>
    internal class LockObject
    {
        private static readonly Timer cleanupTimer;
        //private static readonly TimeSpan cleanupInterval = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan lockExpiryTime = TimeSpan.FromMinutes(10);
        private static readonly Dictionary<int, LockInfo> fileLocks = new Dictionary<int, LockInfo>();

        static LockObject()
        {
            cleanupTimer = new Timer(CleanupExpiredLocks, null, TimeSpan.Zero, TimeSpan.FromMinutes(3));
        }
        private static object _lockObject = new object();
        public static LockInfo GetForLock(string key) //=> fileLocks.GetOrAdd(key.GetHashCode(), _ => new LockInfo());
        {
            int cdk = key.GetHashCode();
            if (!fileLocks.ContainsKey(cdk))
                lock (_lockObject)
                    if (!fileLocks.ContainsKey(cdk))
                        fileLocks[cdk] = new LockInfo();
            var ret = fileLocks[cdk];
            ret.LastAccessTime = DateTime.UtcNow;
            return ret;
        }
        private static void CleanupExpiredLocks(object state)
        {
            foreach (var obj in fileLocks)
            {
                if ((DateTime.UtcNow - obj.Value.LastAccessTime) > lockExpiryTime)
                {
                    fileLocks.Remove(obj.Key);
                }
            }
        }
        internal class LockInfo
        {
            //public object Lock { get; }
            //public bool IsLocked { get; set; }
            public DateTime LastAccessTime { get; set; }
            public LockInfo()
            {
                //Lock = new object();
                LastAccessTime = DateTime.UtcNow;
            }
        }
    }
}
