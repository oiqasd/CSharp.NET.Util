using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public sealed class LimitTask : TokenBucket
    {
        internal DateTime lastReadTime { get; set; } = DateTime.UtcNow;
        internal bool Lts { get; set; } = false;
        public LimitTask(int capacity = 0)
        {
            SetCapacity(capacity);
            var t = new Thread(build);
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }
        /// <summary>
        /// 等待补充
        /// </summary>
        public void Wait()
        {
            lastReadTime = DateTime.UtcNow;
            while (bucket == null || bucket.Count == 0)
                Task.Delay(20).Wait();
            bucket.Dequeue();
        }
        /// <summary>
        /// 不等待补充，直接抛出异常
        /// </summary>
        /// <exception cref="AppException"></exception>
        public void Cross()
        {
            lastReadTime = DateTime.UtcNow;
            while (bucket == null || bucket.Count == 0)
                throw new AppException("network is busy");
        }

        public LimitTask SetCapacity(int value)
        {
            setCapacity(value);
            return this;
        }
        public LimitTask SetLts(bool lts = true)
        {
            Lts = lts;
            return this;
        }
    }
}
