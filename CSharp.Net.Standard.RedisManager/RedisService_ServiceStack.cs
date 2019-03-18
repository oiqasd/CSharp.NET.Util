using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Redis;

namespace CSharp.Net.Standard.RedisManager
{
    public class RedisService : IRedisService
    {
        /// <summary>
        /// 依赖 ServiceStack.Redis
        /// </summary>
        /// <returns></returns>
        private IRedisClient GetClient()
        { 
            RedisClientManagerConfig config = new RedisClientManagerConfig()
            {
                DefaultDb = RedisServiceConfig.DefaultDb,
                MaxReadPoolSize = RedisServiceConfig.PoolSize,
                MaxWritePoolSize = RedisServiceConfig.PoolSize,
                AutoStart = true
            };

            if (string.IsNullOrEmpty(RedisServiceConfig.Address))
            {
                throw new KeyNotFoundException("没有添加配置参数Address");
            }

            var arrAddress = RedisServiceConfig.Address.Split(',');

            var pool = new PooledRedisClientManager(arrAddress, arrAddress, config);
            return pool.GetClient();
        }

        private void Dispose(IRedisClient redisClient) => redisClient.Dispose();

        public void Set(string key, object obj, TimeSpan expiresIn)
        {
            using (var _client = GetClient())
            {
                _client.Set(key, obj, expiresIn);
            }
        }

        public void Set(string key, object obj, DateTime expiresAt)
        {
            using (var _client = GetClient())
            {
                _client.Set(key, obj, expiresAt);
            }
        }

        public void PrependItemToList(string key, string obj)
        {
            using (var _client = GetClient())
            {
                _client.PrependItemToList(key, obj);
            }
        }

        public void PrependRangeToList(string key, List<string> values)
        {
            using (var _client = GetClient())
            {
                _client.PrependRangeToList(key, values);
            }
        }

        public string PopItemFromList(string listId)
        {
            using (var _client = GetClient())
            {
                return _client.PopItemFromList(listId);
            }
        }

        public long GetListCount(string listId)
        {
            using (var _client = GetClient())
            {
                return _client.GetListCount(listId);
            }
        }

        public void Remove(string key)
        {
            using (var _client = GetClient())
            {
                _client.Remove(key);
            }
        }

        public void RemoveAll(IEnumerable<string> key)
        {
            using (var _client = GetClient())
            {
                _client.RemoveAll(key);
            }
        }

        public T Get<T>(string key)
        {
            using (var _client = GetClient())
            {
                return _client.Get<T>(key);
            }
        }
    }
}
