namespace CSharp.Net.Standard.RedisManager
{
    public class RedisServiceConfig
    {
        /// <summary>
        /// Redis服务器端的IP地址
        /// 如： 192.168.8.1:6379,192.168.8.2:6379,192.168.8.3:6379
        /// redis0:6380,redis1:6380,allowAdmin=true,ssl=true
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 连接池连接数
        /// </summary>
        public int PoolSize { get; set; } = 20;
        /// <summary>
        /// 默认0
        /// </summary>
        public int DefaultDb { get; set; } = 0;

        /// <summary>
        /// redis 密码
        /// </summary>
        public string Pwd { get; set; }
    }
}
