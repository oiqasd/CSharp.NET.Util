using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CSharp.Net.Util
{
    public static class TimeSource
    {
        /// <summary>
        /// Stopwatch 每秒 Tick 数
        /// </summary>
        public static readonly long Frequency = Stopwatch.Frequency;

        /// <summary>
        /// 将TimeSpan的Ticks值转换为Stopwatch的计数值
        /// </summary>
        public static readonly long TickToStopwatch =
            Stopwatch.Frequency / TimeSpan.TicksPerSecond;

        /// <summary>
        /// Stopwatch tick -> 毫秒
        /// </summary>
        public static readonly long MillisecondTicks = Frequency / 1000;

        /// <summary>
        /// 获取高精度计数值
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTimestamp()
            => Stopwatch.GetTimestamp();
        /// <summary>
        /// 计数值转毫秒
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns>毫秒</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TicksToMs(long ticks)
                => (int)(ticks * 1000 / Frequency);
    }
}
