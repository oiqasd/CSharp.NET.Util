using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 雪花算法
    /// </summary>
    public sealed class IdWorkerHelper
    {
        private static IdWorker _IdWorkInstance = null;
        private static object _obj = new object();

        /// <summary>
        /// work实例
        /// </summary>
        public static IdWorker Instance => _IdWorkInstance;

        /// <summary>
        /// 设置参数，建议程序初始化时执行一次
        /// </summary>
        /// <param name="options"></param>
        public static void GeneratorIdWorker(IdWorkerOptions options = null)
        {
            if (_IdWorkInstance == null)
            {
                lock (_obj)
                {
                    if (_IdWorkInstance == null)
                    {
                        if (options == null)
                        {
                            options = new IdWorkerOptions();
                            string ip = IpUtil.GetLocalIP();
                            if (IpUtil.CheckIp(ip))
                            {
                                options.MachineId = ConvertHelper.ConvertTo<ushort>(ip.Split('.')[3], 1);
                            }
                            //var id = ((0x000000FF & (long)mac[mac.length - 2]) | (0x0000FF00 & (((long)mac[mac.length - 1]) << 8))) >> 6;
                            //id = id % (maxDatacenterId + 1);
                        }
                        _IdWorkInstance = new IdWorker(options);
                    }
                }
            }

        }
    }

    /// <summary>
    /// 雪花算法
    /// </summary>
    public sealed class IdWorker
    {
        /// <summary>
        /// 机器ID
        /// </summary>
        private long machineId;
        /// <summary>
        /// 唯一时间，这是一个避免重复的随机量，自行设定不要大于当前时间戳
        /// 默认2020-01-01 00:00:00
        /// </summary>
        private long twepoch = 1577808000000L;
        /// <summary>
        /// id序号
        /// </summary>
        private long sequence = 0L;
        /// <summary>
        /// 机器码字节数。4个字节用来保存机器码(定义为Long类型会出现，最大偏移64位，所以左移64位没有意义)
        /// </summary>
        private int workerIdBits = 4;
        /// <summary>
        /// 最大机器ID
        /// </summary>
        private long maxMachineId = -1L ^ -1L << 4;//-1L ^ -1L << workerIdBits;
        /// <summary>
        /// 计数器字节数，10个字节用来保存计数码
        /// </summary>
        private int sequenceBits = 10;
        /// <summary>
        /// 机器码数据左移位数，就是后面计数器占用的位数
        /// </summary>
        private int workerIdShift = 10;// sequenceBits;
        /// <summary>
        /// 时间戳左移动位数就是机器码和计数器总字节数
        /// </summary>
        private int timestampLeftShift = 14;// sequenceBits + workerIdBits;
        /// <summary>
        /// 一毫秒内可以产生计数，如果达到该值则等到下一毫秒再进行生成
        /// 默认1023
        /// </summary>
        private long sequenceMax = -1L ^ (-1L << 10);//-1L ^ (-1L << sequenceBits);
        private long lastTimestamp = -1L;

        /// <summary>
        /// 机器码
        /// </summary>
        /// <param name="machineId"></param>
        public IdWorker(int machineId = 1)
        {
            if (machineId > maxMachineId || machineId < 0)
                throw new WorkIdException($"machineId can't be greater than {maxMachineId} or less than 0 ");
            this.machineId = machineId;
        }

        public IdWorker(IdWorkerOptions options)
        {
            machineId = options.MachineId;
            twepoch = DateTimeHelper.GetTimeStampLong(options.BaseUtcTime);
            workerIdBits = options.WorkerIdBits;
            sequenceBits = options.SequenceBits;
            if (options.SequenceMask <= 0)
                options.SequenceMask = -1L ^ (-1L << options.SequenceBits);
            maxMachineId = -1L ^ -1L << workerIdBits;
            timestampLeftShift = sequenceBits + workerIdBits;
            sequenceMax = options.SequenceMask;
            workerIdShift = sequenceBits;

            if (machineId > maxMachineId || machineId < 0)
                throw new WorkIdException($"machineId can't be greater than {maxMachineId} or less than 0 ");

            timestampLeftShift = (int)Math.Ceiling(Math.Log((machineId << workerIdShift) | sequenceMax, 2));
        }

        /// <summary>
        /// 生成唯一Id
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long NextId()
        {
            lock (this)
            {
                long timestamp = timeGen();
                int r = 0;
                while (timestamp < lastTimestamp)
                {
                    //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常
                    Thread.Sleep(1);
                    timestamp = tillNextMillis(this.lastTimestamp);
                    r++;
                    if (r > 1000 * 10)
                    {
                        LogHelper.Fatal("WorkId Error", string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds", this.lastTimestamp - timestamp));
                        return Guid.NewGuid().GetHashCode();
                        //throw new WorkIdException(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds", this.lastTimestamp - timestamp));
                    }
                }

                if (this.lastTimestamp == timestamp)
                {
                    //同一毫秒中生成ID
                    sequence = (sequence + 1) & sequenceMax; //用&运算计算该毫秒内产生的计数是否已经到达上限
                    if (sequence == 0)
                    {
                        //一毫秒内产生的ID计数已达上限，等待下一毫秒
                        timestamp = tillNextMillis(this.lastTimestamp);
                    }
                }
                else
                {
                    sequence = 0; //计数清0
                }
                this.lastTimestamp = timestamp;
                long nextId = ((timestamp - twepoch) << timestampLeftShift) | (machineId << workerIdShift) | sequence;
                return nextId;
            }
        }

        /// <summary>
        /// 获取下一毫秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private long tillNextMillis(long lastTimestamp)
        {
            long timestamp = timeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = timeGen();
            }
            return timestamp;
        }

        /// <summary>
        /// 生成当前毫秒时间戳
        /// </summary>
        /// <returns></returns>
        private long timeGen()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
