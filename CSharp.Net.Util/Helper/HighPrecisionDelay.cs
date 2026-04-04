using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 高性能线程延迟，减少Task.Delay抖动
    /// 直接替换Task.Delay()
    /// </summary>
    public static class HighPrecisionDelay
    {
        /// <summary>
        /// 小于该时间进入自旋等待
        /// </summary>
        private const int SpinThresholdMs = 2;

        public static async ValueTask WaitAsync(
            long waitStopwatchTicks,
            CancellationToken ct = default)
        {
            if (waitStopwatchTicks <= 0)
                return;

            long start = Stopwatch.GetTimestamp();
            long target = start + waitStopwatchTicks;

            int waitMs = TimeSource.TicksToMs(waitStopwatchTicks);
                
            // =====================
            // Phase 1: Sleep
            // =====================
            if (waitMs > SpinThresholdMs)
            {
                await Task.Delay(
                    waitMs - SpinThresholdMs,
                    ct);
            }

            // =====================
            // Phase 2: Spin
            // =====================
            SpinWait spin = new();

            while (Stopwatch.GetTimestamp() < target)
            {
                ct.ThrowIfCancellationRequested();
                spin.SpinOnce();
            }
        }
    }
}
