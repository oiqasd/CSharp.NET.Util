using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    internal sealed class RateLimiter
    {
        private readonly TokenBucket? _single;
        private readonly TokenBucket[]? _multi;
        internal bool _Lts { get; set; } = false;
        internal int _lastReadTick;

        //~RateLimiter(){Console.WriteLine("RateLimiter finalized");}


        /// <summary>
        /// 示例
        /// <code>
        /// var limiter = new RateLimiter(new[]
        /// {
        ///   (limit:  5000, interval: TimeSpan.FromSeconds(1)),//秒
        ///   (limit: 10000, interval: TimeSpan.FromMinutes(1)) //分
        /// });
        /// </code>
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="lts"></param>
        internal RateLimiter(IEnumerable<(int limit, TimeSpan interval)> rules, bool lts = false)
        {
            _Lts = lts;
            var list = new List<TokenBucket>();
            _lastReadTick = Environment.TickCount;
            foreach (var r in rules)
            {
                list.Add(new TokenBucket(r.limit, r.interval));
            }

            if (list.Count == 1)
            {
                _single = list[0];
            }
            else
            {
                _multi = list.ToArray();
            }
        }

        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="permits">一次获取配额数，默认1</param>
        /// <returns>true:成功，false:失败</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAcquire(int permits = 1)
        {
            _lastReadTick = Environment.TickCount;
            if (_single != null)
            {
                return _single.TryAcquire(permits);
            }

            var buckets = _multi!;
            for (int i = 0; i < buckets.Length; i++)
            {
                if (!buckets[i].TryAcquire(permits))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 无限等待
        /// </summary>
        /// <param name="permits">一次获取配额数，默认1</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AcquireWait(int permits = 1)
        {
            _lastReadTick = Environment.TickCount;
            if (_single != null)
            {
                await _single.AcquireAsync(permits);
                return;
            }
            var buckets = _multi!;
            for (int i = 0; i < buckets.Length; i++)
            {
                await buckets[i].AcquireAsync(permits);
            }
        }

        /// <summary>
        /// 长期保持容量
        /// </summary>
        /// <param name="lts">true:不会被回收，false:闲置10分钟将被自动回收，回收后需重新设置大小</param>
        /// <returns></returns>
        public RateLimiter SetLts(bool lts = true)
        {
            _Lts = lts;
            return this;
        }

        /*
        /// <summary>
        /// 设置容量
        /// </summary>
        /// <param name="value">参数个数必须于组合顺序一致</param>
        /// <returns></returns>
        public RateLimiter SetCapacityForce(params int[] value)
        {
            if (_single != null&&value.Count()>1)
            {
                if (value.Count() != 1)
                    throw new ArgumentException("Limiter count error.");
            }
            var buckets = _multi!;
            for (int i = 0; i < buckets.Length; i++)
            {
             
            }
            return this;
        }
        */
    }
}
