using System;
using System.Threading;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 雪花算法
    /// </summary>
    public sealed class IdWorkerHelper
    {
        private static readonly object SyncRoot = new object();
        private static Snowflake _instance;

        public static Snowflake Instance => Volatile.Read(ref _instance);

        /// <summary>
        /// 设置参数，建议程序初始化时执行一次
        /// </summary>
        /// <param name="options"></param>
        public static void GeneratorIdWorker(IdWorkerOptions options = null)
        {
            if (Instance != null)
                return;

            lock (SyncRoot)
            {
                if (_instance != null)
                    return;

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

                Volatile.Write(ref _instance, new Snowflake(options));

            }
        }
    }
    /// <summary>
    /// 雪花算法
    /// </summary>
    public sealed class Snowflake
    {
        private readonly object _syncRoot = new object();
        /// <summary>
        /// 机器ID
        /// </summary>
        private long _machineId;
        /// <summary>
        /// 唯一时间，这是一个避免重复的随机量，自行设定不要大于当前时间戳
        /// 默认2020-01-01 00:00:00
        /// </summary>
        private long _twepoch = 1577808000000L;
        /// <summary>
        /// id序号
        /// </summary>
        private long _sequence = 0L;

        /// <summary>
        /// 机器码数据左移位数，后面计数器占用的位数
        /// </summary>
        private int _workerIdShift;//= _sequenceBits;
        /// <summary>
        /// 时间戳左移动位数,机器码和计数器总字节数
        /// </summary>
        private int _timestampShift;// = _sequenceBits + _workerIdBits;
        /// <summary>
        /// 支持的最大序列id数量.
        /// 一毫秒内可以产生计数，如果达到该值则等到下一毫秒再进行生成
        /// 默认1023
        /// </summary>
        private long _sequenceMax;//= -1L ^ (-1L << _sequenceBits);
        private long _lastTimestamp = -1L;
        //数据中心配置
        //int datacenterIdBits = 1;// 数据中心id所占位数
        //int datacenterIdShift = sequenceBits + workerIdBits;// 数据中心id左移位数
        //long datacenterIdMax = -1L ^ (-1L << datacenterIdBits));// 支持的最大数据中心id数量
        private WorkerType _workerType = WorkerType.Millisecond;
        /// <summary>
        /// 机器码
        /// </summary>
        /// <param name="machineId"></param>
        public Snowflake(int machineId = 1) : this(new IdWorkerOptions { MachineId = machineId })
        {

        }

        public Snowflake(IdWorkerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.WorkerIdBits <= 0 || options.WorkerIdBits >= 63)
                throw new ArgumentOutOfRangeException(nameof(options.WorkerIdBits), "WorkerIdBits must be between 1 and 62.");
            if (options.SequenceBits <= 0 || options.SequenceBits >= 63)
                throw new ArgumentOutOfRangeException(nameof(options.SequenceBits), "SequenceBits must be between 1 and 62.");
            if (options.WorkerIdBits + options.SequenceBits >= 63)
                throw new ArgumentException("WorkerIdBits and SequenceBits must use fewer than 63 bits.", nameof(options));

            long maxMachineId = (1L << options.WorkerIdBits) - 1;// -1L ^ -1L << workerIdBits;
            if (options.MachineId < 0 || options.MachineId > maxMachineId)
                throw new ArgumentOutOfRangeException(nameof(options.MachineId), options.MachineId,
                    $"MachineId must be between 0 and {maxMachineId}.");

            long sequenceMask = (1L << options.SequenceBits) - 1;
            long sequenceMax = options.SequenceMax <= 0 ? sequenceMask : options.SequenceMax;
            if (sequenceMax > sequenceMask)
                throw new ArgumentOutOfRangeException(nameof(options.SequenceMax), options.SequenceMax,
                    $"SequenceMax cannot be greater than {sequenceMask} for {options.SequenceBits} bits.");

            _machineId = options.MachineId;
            _workerType = options.WorkerType;
            _twepoch = DateTimeHelper.GetTimestamp(options.BaseUtcTime, options.WorkerType == WorkerType.Millisecond);
            _workerIdShift = options.SequenceBits;
            _timestampShift = options.SequenceBits + options.WorkerIdBits;//(int)Math.Ceiling(Math.Log((machineId << workerIdShift) | sequenceMax, 2));
            _sequenceMax = sequenceMax;

            if (_twepoch > DateTimeHelper.GetTimestamp(_workerType == WorkerType.Millisecond))
                throw new ArgumentOutOfRangeException(nameof(options.BaseUtcTime), "BaseUtcTime cannot be later than the current time.");
        }

        /// <summary>
        /// 生成唯一Id
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long NextId(DateTime? baseTime = null)
        {
            lock (_syncRoot)
            {
                long timestamp = DateTimeHelper.GetTimestamp(_workerType == WorkerType.Millisecond);
                int r = 0;
                while (timestamp < _lastTimestamp)
                {
                    //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常
                    Thread.Sleep(1);
                    timestamp = TillNextMillis(_lastTimestamp);
                    r++;
                    if (r > 1000 * 10)
                    {
                        LogHelper.Fatal("WorkId Error",
                            string.Format("Clock moved backwards.Refusing to generate id for {0} milliseconds", _lastTimestamp - timestamp));
                        // return Guid.NewGuid().GetHashCode();
                        throw new InvalidOperationException(
                         $"Clock moved backwards by {_lastTimestamp - timestamp} time unit(s); refusing to generate an ID.");
                    }
                }

                if (_lastTimestamp == timestamp)
                {
                    //同一毫秒中生成ID
                    //sequence = (sequence + 1) & sequenceMax; //用&运算计算该毫秒内产生的计数是否已经到达上限             
                    //if (sequence == 0)
                    _sequence++;
                    if (_sequence > _sequenceMax)
                    {
                        _sequence = 0;
                        //一毫秒内产生的ID计数已达上限，等待下一毫秒
                        timestamp = TillNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0; //计数清0
                }
                long epoch = baseTime.HasValue
                    ? DateTimeHelper.GetTimestamp(baseTime.Value, _workerType == WorkerType.Millisecond)
                    : _twepoch;
                long elapsed = timestamp - epoch;

                if (elapsed > (long.MaxValue >> _timestampShift))
                    throw new InvalidOperationException("The configured Snowflake timestamp range has been exhausted.");
                var nextId = (elapsed << _timestampShift) | (_machineId << _workerIdShift) | _sequence;//|(s.datacenterId << datacenterIdShift)
                _lastTimestamp = timestamp;
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
            long timestamp;
            do
            {
                Thread.Yield();
                timestamp = DateTimeHelper.GetTimestamp(_workerType == WorkerType.Millisecond);
            }
            while (timestamp <= lastTimestamp);
            return timestamp;
        }

    }
}
