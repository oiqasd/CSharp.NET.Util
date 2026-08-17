using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public sealed class TokenBucket
    {
        private long _capacity;//容量
        private long _tokensPerTick;
        private long _tokens;
        private long _lastTimestamp;
        private long _intervalTicks; //补充频率
        private long _tokensPerInterval;//每次补充数量

        public TokenBucket(long rate, TimeSpan interval)
        {
            _capacity = rate;
            _intervalTicks = TimeSource.Frequency * interval.Ticks / TimeSpan.TicksPerSecond;
            _tokensPerInterval = rate;
            _tokensPerTick = rate * TimeSource.Frequency / interval.Ticks;
            _tokens = rate;
            _lastTimestamp = TimeSource.GetTimestamp();
        }

        /// <summary>
        /// 不等待立即返回
        /// </summary>
        /// <param name="permits"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAcquire(long permits)
        {
            if (permits < 0)
                throw new ArgumentOutOfRangeException(nameof(permits), permits, "Permits cannot be negative.");
            IntervalRefill();
            /*
              long after = Interlocked.Add(ref _tokens, -permits);
              if (after >= 0)return true;
              // 回滚
              Interlocked.Add(ref _tokens, permits);return false;
            */
            while (true)
            {
                long current = Volatile.Read(ref _tokens);
                if (current < permits) return false;

                if (Interlocked.CompareExchange(ref _tokens, current - permits, current) == current) return true;
            }
        }

        /// <summary>
        /// 异步等待获取
        /// </summary>
        /// <param name="permits"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async ValueTask AcquireAsync(long permits, CancellationToken ct = default)
        {
            if (permits < 0)
                throw new ArgumentOutOfRangeException(nameof(permits), permits, "Permits cannot be negative.");
            SpinWait spin = new();
            while (true)
            {
                IntervalRefill();
                long current = Volatile.Read(ref _tokens);

                if (current >= permits)
                {
                    if (Interlocked.CompareExchange(ref _tokens, current - permits, current) == current)
                        return;
                }

                long deficit = permits - current;
                if (deficit <= 0) return;

                //离散等待时间计算 
                long intervalsNeeded = (deficit + _tokensPerInterval - 1) / _tokensPerInterval;
                long next = Volatile.Read(ref _lastTimestamp);
                long target = next + (intervalsNeeded - 1) * _intervalTicks;
                long waitTicks = target - TimeSource.GetTimestamp();

                //连续生成等待时间计算，LazyRefill
                //long waitTicks = deficit * TimeSource.Frequency / _tokensPerTick;

                int waitMs = TimeSource.TicksToMs(waitTicks);
                if (waitMs <= 1)
                {
                    spin.SpinOnce();
                    continue;
                }
                await Task.Delay(waitMs, ct);
            }
        }

        /*
        /// <summary>
        /// 按速率连续生成
        /// </summary>
        private void LazyRefill()
        {
            long now = TimeSource.GetTimestamp();
            while (true)
            {
                long last = Volatile.Read(ref _lastTimestamp);

                long elapsed = now - last;
                if (elapsed <= 0)
                    return;

                long add = elapsed * _tokensPerTick / TimeSource.Frequency;
                if (add <= 0)
                    return;

                long newLast = last + add * TimeSource.Frequency / _tokensPerTick;

                if (Interlocked.CompareExchange(ref _lastTimestamp, newLast, last) != last)
                {
                    continue;
                }
                AddTokens(add);
                return;
            }
        }*/

        /// <summary>
        /// 离散生成(非连续 一次生成token)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IntervalRefill()
        {
            long now = TimeSource.GetTimestamp();
            while (true)
            {
                long last = Volatile.Read(ref _lastTimestamp);
                if (now < last)
                    return;
                long intervals = (now - last) / _intervalTicks + 1;
                long newNext = last + intervals * _intervalTicks;
                if (Interlocked.CompareExchange(ref _lastTimestamp, newNext, last) != last)
                    continue;
                var add = intervals * _tokensPerInterval;
                AddTokens(add);
                return;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddTokens(long add)
        {
            long oldTokens, newTokens;
            do
            {
                oldTokens = Volatile.Read(ref _tokens);
                newTokens = Math.Min(_capacity, oldTokens + add);
            }
            while (Interlocked.CompareExchange(ref _tokens, newTokens, oldTokens) != oldTokens);

            /*
            while (true)
            {
                long current =Volatile.Read(ref _tokens);
                long next = current + add;
                if (next > _capacity)
                    next = _capacity;

                if (Interlocked.CompareExchange(ref _tokens, next, current) == current)
                    return;
            }*/
        }


        /// <summary>
        /// 热更新速率
        /// </summary>
        public void UpdateRate(int newCapacity, TimeSpan newWindow)
        {
            //lock (_lock)
            {
                IntervalRefill();
                _capacity = newCapacity;

                if (_tokens > newCapacity)
                    _tokens = newCapacity;
            }
        }
        /*
        internal Queue<byte> bucket = null;
        int _capacity = 50;
        internal const int MaxCap = 1000000;
        internal const int MinCap = 1;
        internal void build()
        {
            bucket = bucket ?? new Queue<byte>(_capacity);
            while (true)
            {
                int cnt = bucket.Count;
                if (cnt < _capacity)
                    while (cnt++ < _capacity)
                        bucket.Enqueue(0);
                Task.Delay(1000);
            }
        }
        /// <summary>
        /// 默认50，每秒补充50个令牌
        /// </summary>
        /// <param name="value"></param>
        internal void setCapacity(int value)
        {
            if (value < MinCap) return;
            if (value > MaxCap) return;
            _capacity = value;
            if (bucket != null && _capacity < (bucket.Count() / 3))
                bucket = new Queue<byte>(_capacity);
            //bucket.Clear();
        }*/
    }
}
