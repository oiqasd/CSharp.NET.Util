using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    public class IdWorkerOptions
    {
        /// <summary>
        /// 机器码
        /// 最大值 2^workerIdBits-1
        /// </summary>
        public virtual int MachineId { get; set; } = 1;

        /// <summary>
        /// 机器码字节数
        /// </summary>
        public virtual int WorkerIdBits { get; set; } = 8;

        /// <summary>
        /// 计数器字节数
        /// </summary>
        public virtual int SequenceBits { get; set; } = 10;

        /// <summary>
        /// 基础时间（UTC格式）
        /// 不能超过当前系统时间
        /// </summary>
        public virtual DateTime BaseUtcTime { get; set; } = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 一毫秒内可以产生计数，如果达到该值则等到下一毫秒再进行生成
        /// 默认: -1L ^ -1L 《 SequenceBits
        /// </summary>
        public virtual long SequenceMask { get; set; }
    }
}
