using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 
    /// </summary>
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
        /// <exception cref="InvalidOperationException"></exception>
        public void Cross()
        {
            lastReadTime = DateTime.UtcNow;
            while (bucket == null || bucket.Count == 0)
                throw new InvalidOperationException("Network is busy.");
        }
        /// <summary>
        /// 设置容量，默认50
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public LimitTask SetCapacity(int value = 50)
        {
            setCapacity(value);
            return this;
        }

        /// <summary>
        /// 长期保持容量
        /// </summary>
        /// <param name="lts">true:不会被回收，false:闲置10分钟将被自动回收，回收后需重新设置大小</param>
        /// <returns></returns>
        public LimitTask SetLts(bool lts = true)
        {
            Lts = lts;
            return this;
        }
    }
}
