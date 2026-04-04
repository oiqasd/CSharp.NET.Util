using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 限流组件
    /// <para> 限制以每第一次调用 <see cref="Init()"/> 的配置为准</para>
    /// <para> 默认回收 60s 未使用的对象，若要长期保持，请设置 _Lts </para>
    /// </summary>
    public sealed class LimitHelper
    {
        static readonly ConcurrentDictionary<string, Lazy<RateLimiter>> _tenants = new();

        private static readonly TimeSpan cleanupInterval = TimeSpan.FromSeconds(5);
        private static readonly int _lockExpiryTick = 60_000;//60秒
        private static readonly Timer cleanupTimer;

        static LimitHelper()
        {
            cleanupTimer = new Timer(CleanupExpiredLocks, null, cleanupInterval, cleanupInterval);
        }

        /// <summary>
        /// 限流配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secondRate">秒 默认50</param>
        /// <param name="minuteRate">分钟组合 默认不生效</param>
        /// <param name="lts">是否长期保持</param>
        /// <returns></returns>
        static RateLimiter GetLimiter(string key, int secondRate = 50, int minuteRate = -1, bool lts = false)
        {
            var r = _tenants.GetOrAdd(key, _ => new Lazy<RateLimiter>(() =>
            {
                if (minuteRate < 0)
                    return new RateLimiter(new[] { (secondRate, TimeSpan.FromSeconds(1)) }, lts);

                return new RateLimiter(new[] {(secondRate, TimeSpan.FromSeconds(1)),
                                              (minuteRate, TimeSpan.FromMinutes(1))}, lts);
            }, LazyThreadSafetyMode.ExecutionAndPublication));
            var d = r.Value;
            return d;
        }

        /// <summary>
        /// 按秒限流
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secondRate">按秒限流</param>
        /// <param name="lts"></param>
        public static void Init(string key, int secondRate = 100, bool lts = true)
            => GetLimiter(key, secondRate, lts: lts);

        /// <summary>
        /// 分秒组合限流
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secondRate">每秒限制</param>
        /// <param name="minuteRate">每分限制</param>
        /// <param name="lts"></param>
        public static void Init(string key, int secondRate, int minuteRate, bool lts = true)
            => GetLimiter(key, secondRate, minuteRate, lts: lts);

        /// <summary>
        /// 获取token，等待直到获取成功
        /// </summary>
        /// <param name="key"></param>
        /// <param name="permits">一次消耗数量</param>
        public static Task AcquireWait(string key, int permits = 1)
            => GetLimiter(key).AcquireWait(permits);

        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="key"></param>
        /// <param name="permits"></param>
        /// <returns>true：成功，false：失败</returns>
        public static bool TryAcquire(string key, int permits = 1)
            => GetLimiter(key).TryAcquire(permits);

        private static void CleanupExpiredLocks(object state)
        {
            foreach (var kvp in _tenants)
            {
                if (!kvp.Value.IsValueCreated)
                {
                    _tenants.TryRemove(kvp.Key, out _);
                    continue;
                }
                if (!kvp.Value.Value._Lts && (Environment.TickCount - kvp.Value.Value._lastReadTick) > _lockExpiryTick)
                {
#if NET5_0_OR_GREATER
                    _tenants.TryRemove(kvp);
#else
                    _tenants.TryRemove(kvp.Key, out _);
#endif
                }
            }
        }
    }
}
