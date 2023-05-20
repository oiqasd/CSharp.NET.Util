using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace CSharp.Net.Cache.Redis
{
    public class RedisCacheManager : IDisposable
    {

        protected volatile IConnectionMultiplexer _connection;
        protected IDatabase _db;
        private bool _disposed;

        protected readonly RedisCacheOptions _options;
        private readonly string _instance;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        //多库配置
        //private readonly ConcurrentDictionary<string, Lazy<IConnectionMultiplexer>> ConnectionCache = new ConcurrentDictionary<string, Lazy<IConnectionMultiplexer>>();

        public RedisCacheManager(IOptions<RedisCacheOptions> options)
        {
            try
            {
                Monitor.Enter(this);

                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                _options = options.Value;
                _instance = _options.InstanceName ?? string.Empty;

                Connect();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        private void Connect()
        {
            CheckDisposed();
            if (_db != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_db == null)
                {
                    if (_options.ConnectionMultiplexerFactory == null)
                    {
                        if (_options.ConfigurationOptions != null)
                        {
                            _connection = ConnectionMultiplexer.Connect(_options.ConfigurationOptions);
                        }
                        else
                        {
                            var option = ConfigurationOptions.Parse(_options.ConnectionString);
                            //option.AsyncTimeout = option.AsyncTimeout < 5000 ? 5000 : option.AsyncTimeout;
                            //option.ConnectTimeout = option.ConnectTimeout < 15000 ? 15000 : option.ConnectTimeout;
                            //option.AbortOnConnectFail = false;
                            _connection = ConnectionMultiplexer.Connect(option);
                        }
                        _connection.ConnectionFailed += MuxerConnectionFailed;
                        _connection.ConnectionRestored += MuxerConnectionRestored;
                        _connection.ErrorMessage += MuxerErrorMessage;
                        _connection.ConfigurationChanged += MuxerConfigurationChanged;
                        _connection.ConfigurationChangedBroadcast += MuxerConfigurationChangedBroadcast;
                        _connection.HashSlotMoved += MuxerHashSlotMoved;
                        _connection.InternalError += MuxerInternalError;
                    }
                    else
                    {
                        _connection = _options.ConnectionMultiplexerFactory().GetAwaiter().GetResult();
                    }

                    TryRegisterProfiler();
                    _db = _connection.GetDatabase(_options.DefaultDB);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _connection?.Dispose();
            _connection?.Close();
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        private void TryRegisterProfiler()
        {
            if (_connection != null && _options.ProfilingSession != null)
            {
                _connection.RegisterProfiler(_options.ProfilingSession);
            }
        }

        /// <summary>
        /// 获取集群服务器
        /// </summary>
        private EndPoint[] GetEndPoints()
        {
            return _connection.GetEndPoints();
        }

        /// <summary>
        /// 创建事务
        /// </summary>
        /// <returns></returns>
        protected ITransaction CreateTransaction()
        {
            return _db.CreateTransaction();
        }

        /// <summary>
        /// 获取指定服务器
        /// </summary>
        /// <param name="hostAndPort"></param>
        /// <returns></returns>
        protected IServer GetServer(string hostAndPort)
        {
            return _connection.GetServer(hostAndPort);
        }

        /// <summary>
        /// 获取指定服务器
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        protected IServer GetServer(EndPoint endPoint)
        {
            return _connection.GetServer(endPoint);
        }

        /// <summary>
        /// 获取string所有key
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllKeys()
        {
            List<string> keys = new List<string>();
            foreach (var point in GetEndPoints())
            {
                //获取指定服务器
                var server = GetServer(point);
                //在指定服务器使用keys或scan命令遍历key
                foreach (var key in server.Keys(_options.DefaultDB, "*"))
                {
                    if (!keys.Contains(key))
                        keys.Add(key);
                }
            }
            return keys;
        }

        /// <summary>
        /// 删除当前数据库所有key
        /// </summary>
        /// <returns></returns>
        public List<string> FlushCurrentDatabase()
        {
            List<string> keys = new List<string>();
            foreach (var point in GetEndPoints())
            {
                //获取指定服务器
                var server = _connection.GetServer(point);

                server.FlushDatabase(_options.DefaultDB);
            }
            return keys;
        }

        #region 事件

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {

            Trace.WriteLine("Configuration changed: " + e.EndPoint);
        }

        /// <summary>
        /// 重新配置广播（通常意味着主从同步更改）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChangedBroadcast(object sender, EndPointEventArgs e)
        {

            Trace.WriteLine("Configuration Broadcast: " + e.EndPoint);
        }

        /// <summary>
        /// 发生内部错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Trace.WriteLine("ErrorMessage: " + e.Message);
        }

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Trace.WriteLine("ConnectionRestored: " + e.EndPoint);
        }

        /// <summary>
        /// 连接失败 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Trace.WriteLine("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Trace.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            Trace.WriteLine("InternalError:Message" + e.Exception.Message);
        }

        #endregion 事件

    }
}
