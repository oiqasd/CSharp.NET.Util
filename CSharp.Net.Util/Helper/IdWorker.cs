using System;
using System.Threading;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 雪花算法
    /// </summary>
    public sealed class IdWorkerHelper
    {
        private static Snowflake _IdWorkInstance = null;
        private static object _obj = new object();

        /// <summary>
        /// work实例
        /// </summary>
        public static Snowflake Instance => _IdWorkInstance;

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
                            if (options.MachineId < 1 && IpUtil.CheckIp(ip))
                            {
                                options.MachineId = ConvertHelper.ConvertTo<ushort>(ip.Split('.')[3], 1);
                            }
                            //var id = ((0x000000FF & (long)mac[mac.length - 2]) | (0x0000FF00 & (((long)mac[mac.length - 1]) << 8))) >> 6;
                            //id = id % (maxDatacenterId + 1);
                        }
                        _IdWorkInstance = new Snowflake(options);
                    }
                }
            }

        }
    }

    /// <summary>
    /// 雪花算法
    /// </summary>
    public sealed class Snowflake
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
        /// 机器码字节数,用来保存机器码(最大偏移63位)
        /// </summary>
        private static int workerIdBits = 8;
        /// <summary>
        /// 最大机器ID
        /// </summary>
        private static long maxMachineId = -1L ^ -1L << workerIdBits;
        /// <summary>
        /// 计数器字节数，默认10字节
        /// </summary>
        private static int sequenceBits = 10;
        /// <summary>
        /// 机器码数据左移位数，后面计数器占用的位数
        /// </summary>
        private static int workerIdShift = sequenceBits;
        /// <summary>
        /// 时间戳左移动位数,机器码和计数器总字节数
        /// </summary>
        private static int timestampShift = sequenceBits + workerIdBits;
        /// <summary>
        /// 支持的最大序列id数量.
        /// 一毫秒内可以产生计数，如果达到该值则等到下一毫秒再进行生成
        /// 默认1023
        /// </summary>
        private static long sequenceMax = -1L ^ (-1L << sequenceBits);
        private static long lastTimestamp = -1L;
        //数据中心配置
        //int datacenterIdBits = 1;// 数据中心id所占位数
        //int datacenterIdShift = sequenceBits + workerIdBits;// 数据中心id左移位数
        //long datacenterIdMax = -1L ^ (-1L << datacenterIdBits));// 支持的最大数据中心id数量
        private static WorkerType workerType = WorkerType.Millisecond;
        /// <summary>
        /// 机器码
        /// </summary>
        /// <param name="machineId"></param>
        public Snowflake(int machineId = 1)
        {
            if (machineId > maxMachineId || machineId < 0)
                throw new WorkIdException($"machineId can't be greater than {maxMachineId} or less than 0 ");
            this.machineId = machineId;
        }

        public Snowflake(IdWorkerOptions options)
        {
            machineId = options.MachineId;
            workerType = options.WorkerType;
            twepoch = DateTimeHelper.GetTimestamp(options.BaseUtcTime, options.WorkerType == WorkerType.Millisecond);
            workerIdBits = options.WorkerIdBits;
            sequenceBits = options.SequenceBits;
            if (options.SequenceMax <= 0)
                options.SequenceMax = sequenceMax;
            maxMachineId = -1L ^ -1L << workerIdBits;
            sequenceMax = options.SequenceMax;
            workerIdShift = sequenceBits;

            if (machineId > maxMachineId || machineId < 0)
                throw new WorkIdException($"machineId can't be greater than {maxMachineId} or less than 0 ");

            //timestampShift = sequenceBits + workerIdBits;//+datacenteridBits
            timestampShift = (int)Math.Ceiling(Math.Log((machineId << workerIdShift) | sequenceMax, 2));
        }

        /// <summary>
        /// 生成唯一Id
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long NextId(DateTime? baseTime = null)
        {
            lock (this)
            {
                long timestamp = DateTimeHelper.GetTimestamp(null, workerType == WorkerType.Millisecond);
                int r = 0;
                while (timestamp < lastTimestamp)
                {
                    //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常
                    Thread.Sleep(1);
                    timestamp = TillNextMillis(lastTimestamp);
                    r++;
                    if (r > 1000 * 10)
                    {
                        LogHelper.Fatal("WorkId Error", string.Format("Clock moved backwards.Refusing to generate id for {0} milliseconds", lastTimestamp - timestamp));
                        return Guid.NewGuid().GetHashCode();
                        //throw new WorkIdException(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds", this.lastTimestamp - timestamp));
                    }
                }

                if (lastTimestamp == timestamp)
                {
                    //同一毫秒中生成ID
                    //sequence = (sequence + 1) & sequenceMax; //用&运算计算该毫秒内产生的计数是否已经到达上限             
                    //if (sequence == 0)
                    if((sequence++)> sequenceMax)
                    {
                        sequence = 0;
                        //一毫秒内产生的ID计数已达上限，等待下一毫秒
                        timestamp = TillNextMillis(lastTimestamp);
                    }
                }
                else
                {
                    sequence = 0; //计数清0
                }
                lastTimestamp = timestamp;
                long t = (timestamp - (baseTime.HasValue ? DateTimeHelper.GetTimestamp(baseTime, workerType == WorkerType.Millisecond) : twepoch));
                long nextId = (t << timestampShift) | (machineId << workerIdShift) | sequence;//|(s.datacenterId << datacenterIdShift)

                return nextId;
            }
        }

        /// <summary>
        /// 获取下一毫秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private long TillNextMillis(long lastTimestamp)
        {
            long timestamp = DateTimeHelper.GetTimestamp(null, workerType == WorkerType.Millisecond);
            while (timestamp <= lastTimestamp)
            {
                timestamp = DateTimeHelper.GetTimestamp(null, workerType == WorkerType.Millisecond);
            }
            return timestamp;
        }

    }
}
