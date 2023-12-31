using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Cache.Redis
{
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        RedisCacheOptions IOptions<RedisCacheOptions>.Value { get { return this; } }

        /// <summary>
        /// The Redis profiling session
        /// </summary>
        public Func<ProfilingSession> ProfilingSession { get; set; }
        /// <summary>
        /// 使用已有的实例，null通过配置创建
        /// </summary>
        public Func<Task<IConnectionMultiplexer>> ConnectionMultiplexerFactory { get; set; }

        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// 与ConnectionString参数二选一，优先该配置
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// 默认数据库
        /// <para>default:-1</para>
        /// </summary>
        public int DefaultDB { get; set; } = -1;
        /// <summary>
        /// 最小线程运行数
        /// <para>最小8，默认100</para>
        /// </summary>
        public int MinWorkThread { get; set; } = 100;
        /// <summary>
        /// 最小io线程数
        /// <para>最小1，默认10</para>
        /// </summary>
        public int MinIOThread { get; set; } = 10;
        /// <summary>
        /// 最新线程监控间隔时间
        /// 最小100ms
        /// </summary>
        public int RunThreadIntervalMilliseconds { get; set; } = 2000;
        /// <summary>
        /// dev 输出日志
        /// </summary>
        public string Environment { get; set; }
    }
}
