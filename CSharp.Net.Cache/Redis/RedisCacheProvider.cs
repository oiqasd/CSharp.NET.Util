﻿using CSharp.Net.Util;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CSharp.Net.Cache.Redis
{
    public class RedisCacheProvider : RedisCacheManager, IRedisCache
    {

        public RedisCacheProvider(IOptions<RedisCacheOptions> options) : base(options)
        {

        }

        public object this[string key]
        {
            get { return StringGet<string>(key); }
            set { StringSet(key, value, 0); }
        }

        public int Count => GetAllKeys().Count;

        private static readonly object _lockObj = new object();

        #region String

        #region 同步方法

        /// <summary>
        /// 获取或添加key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func">async()=>{}</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public T GetOrSet<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null) //where T : new()
        {
            var value = StringGet<T>(key);
            if (value != null)
            {
                return value;
            }
            lock (_lockObj)
            {
                value = StringGet<T>(key);
                if (value != null)
                {
                    return value;
                }
                value = func().Result;
                if (value != null)
                {
                    StringSet(key, value, expiry.HasValue ? (int)expiry.Value.TotalSeconds : 0);
                }
            }
            return value;
        }

        /// <summary>
        /// 获取或添加key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public T GetOrSet<T>(string key, Func<T> func, int seconds = 30)
        {
            if (_db.KeyExists(key))
                return StringGet<T>(key);

            //if (LockTake("lck_" + key))
            lock (_lockObj)
            {
                StringSet(key, func.Invoke(), seconds);
            }
            return func.Invoke();
        }

        /// <summary>
        /// 保存单个key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="isSliding"></param>
        public bool StringSet(string key, string value, int cacheSeconds = 0, bool isSliding = true)
        {
            TimeSpan? timeSpan = null;
            if (cacheSeconds > 0)
                timeSpan = TimeSpan.FromSeconds(cacheSeconds);

            return StringSet(key, value, timeSpan);
        }

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiry)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            key = PrefixKey(key);
            return Do(db => db.StringSetAsync(key, value, expiry).Result);
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T obj, TimeSpan? expiry) where T : new()
        => StringSet(key, Serialize(obj), expiry);

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="cacheSeconds"></param>
        /// <returns></returns>
        public bool StringSet(string key, object obj, int cacheSeconds = 0)
        => StringSet(key, Serialize(obj), cacheSeconds);

        /// <summary>
        /// 批量添加 有过期时间
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="cacheSeconds"></param>
        /// <returns></returns>
        public bool StringSet(Dictionary<string, string> keyValues, int cacheSeconds = 0)
        {
            if (keyValues == null || keyValues.Count <= 0)
                throw new ArgumentNullException(nameof(keyValues));

            if (cacheSeconds <= 0)
                return StringSet(keyValues);

            /*
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
            keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(PrefixKey(p.Key), p.Value)).ToList();
            return Do(db => db.StringSet(newkeyValues.ToArray()));
             */

            var batch = _db.CreateBatch();
            Parallel.ForEach(keyValues, str =>
            {
                if (str.Value == null) return;
                batch.StringSetAsync(str.Key, str.Value, cacheSeconds > 0 ? TimeSpan.FromSeconds(cacheSeconds) : TimeSpan.FromSeconds(-1));
            });
            batch.Execute();

            return true;
        }

        /// <summary>
        /// 批量读
        /// </summary>
        public Dictionary<string, string> StringGet(List<string> keys)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            if (keys == null || keys.Count() <= 0)
                throw new ArgumentNullException(nameof(keys));

            var batch = _db.CreateBatch();

            Dictionary<string, Task<RedisValue>> redisValueList = new Dictionary<string, Task<RedisValue>>();
            Parallel.ForEach(keys.Distinct(), str =>
            {
                redisValueList.Add(str, batch.StringGetAsync(PrefixKey(str)));
            });

            batch.Execute();

            Parallel.ForEach(redisValueList, str =>
            {
                data.Add(str.Key, str.Value.Result);
            });
            return data;
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key)
            => Deserialize<T>(StringGet(key));

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public string StringGet(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            key = PrefixKey(key);
            return Do(db => db.StringGetAsync(key).Result);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="val">value</param>
        /// <param name="expireTimeSpan">过期时间</param>
        /// <param name="updateExpire">是否更新过期时间,滑动过期</param>
        /// <returns></returns>
        public double StringIncrement(string key, double val = 1, TimeSpan? expireTimeSpan = null, bool updateExpire = false)
        {
            var pkey = PrefixKey(key);
            var ret = Do(db =>
            {
                double data = db.StringIncrementAsync(pkey, val).Result;
                if (expireTimeSpan != null && (updateExpire || data <= val))
                    db.KeyExpireAsync(pkey, expireTimeSpan);

                return data;
            });
            return ret;
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="val">value</param>
        /// <param name="expireTime">绝对过期时间</param>
        /// <returns></returns>
        public double StringIncrement(string key, double val, DateTime? expireTime)
        {
            if (expireTime.HasValue && expireTime.Value <= DateTime.Now)
                throw new ArgumentException("Expire time can't less now");

            var pkey = PrefixKey(key);
            var ret = Do(db =>
            {
                var data = db.StringIncrementAsync(pkey, val).Result;
                if (expireTime.HasValue)
                    db.KeyExpireAsync(pkey, expireTime.Value);
                return data;
            });

            return ret;
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string key, double val = 1)
        {
            key = PrefixKey(key);
            return Do(db => db.StringDecrementAsync(key, val).Result);
        }

        #endregion 同步方法

        #region 异步方法


        #endregion 异步方法

        #endregion String

        #region List

        #region 同步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRemove<T>(string key, T value)
        {
            key = PrefixKey(key);
            Do(db => db.ListRemoveAsync(key, Serialize(value)).Result);
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T>(string key)
        {
            key = PrefixKey(key);
            return Do(redis =>
            {
                var values = redis.ListRangeAsync(key).Result;
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 右入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRightPush<T>(string key, T value)
        {
            key = PrefixKey(key);
            Do(db => db.ListRightPushAsync(key, Serialize(value)).Result);
        }

        /// <summary>
        /// 右出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string key)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var value = db.ListRightPopAsync(key).Result;
                return Deserialize<T>(value);
            });
        }
        public List<T> ListRightPop<T>(string key, int count)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var value = db.ListRightPopAsync(key, count).Result;
                return ConvetList<T>(value);
            });
        }
        /// <summary>
        /// 左入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListLeftPush<T>(string key, T value)
        {
            key = PrefixKey(key);
            Do(db => db.ListLeftPushAsync(key, Serialize(value)).Result);
        }

        /// <summary>
        /// 左出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string key)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var value = db.ListLeftPopAsync(key).Result;
                return Deserialize<T>(value);
            });
        }
        /// <summary>
        ///  version 6.2.0+
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> ListLeftPop<T>(string key, int count)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var value = db.ListLeftPopAsync(key, count).Result;
                return ConvetList<T>(value);
            });
        }
        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.ListLengthAsync(key).Result);
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRemoveAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return await Do(db => db.ListRemoveAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> ListRangeAsync<T>(string key)
        {
            key = PrefixKey(key);
            var values = await Do(redis => redis.ListRangeAsync(key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return await Do(db => db.ListRightPushAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListRightPopAsync<T>(string key)
        {
            key = PrefixKey(key);
            var value = await Do(db => db.ListRightPopAsync(key));
            return Deserialize<T>(value);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return await Do(db => db.ListLeftPushAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            key = PrefixKey(key);
            var value = await Do(db => db.ListLeftPopAsync(key));
            return Deserialize<T>(value);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> ListLengthAsync(string key)
        {
            key = PrefixKey(key);
            return await Do(redis => redis.ListLengthAsync(key));
        }

        #endregion 异步方法

        #endregion List

        #region Hash

        #region 同步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashExists(string key, string dataKey)
        {
            key = PrefixKey(key);
            return Do(db => db.HashExistsAsync(key, dataKey).Result);
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="expireTime">过期时间</param>
        /// <returns> 返回true表示是新加，返回false表示是修改</returns>
        public bool HashSet<T>(string key, string dataKey, T t, DateTime? expireTime = null)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.HashSetAsync(key, dataKey, Serialize(t)).Result;
                if (expireTime != null)
                {
                    if (expireTime.Value <= DateTime.Now)
                        throw new ArgumentException("Expire time can't less now");

                    db.KeyExpireAsync(key, expireTime.Value);
                }
                return data;
            });
        }
        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="expireTimeSpan">过期时间</param>
        /// <returns> 返回true表示是新加，返回false表示是修改</returns>
        public bool HashSet<T>(string key, string dataKey, T t, TimeSpan? expireTimeSpan)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.HashSetAsync(key, dataKey, Serialize(t)).Result;
                db.KeyExpireAsync(key, expireTimeSpan);
                return data;
            });
        }
        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete(string key, string dataKey)
        {
            key = PrefixKey(key);
            return Do(db => db.HashDeleteAsync(key, dataKey).Result);
        }

        /// <summary>
        /// 移除hash多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public long HashDelete(string key, params object[] dataKeys)
        {
            key = PrefixKey(key);
            //List<RedisValue> dataKeys = new List<RedisValue>() {"1","2"};
            return Do(db => db.HashDeleteAsync(key, ConvertRedisValue(dataKeys)).Result);
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                string value = db.HashGetAsync(key, dataKey).Result;
                return Deserialize<T>(value);
            });
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <param name="expireTime">过期时间</param>
        /// <returns>增长后的值</returns>
        public double HashIncrement(string key, string dataKey, double val = 1, DateTime? expireTime = null)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.HashIncrementAsync(key, dataKey, val).Result;
                if (expireTime.HasValue)
                    db.KeyExpireAsync(key, expireTime.Value);
                return data;
            });
        }

        public double HashIncrement(string key, string dataKey, double val, TimeSpan expireTime)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.HashIncrementAsync(key, dataKey, val).Result;
                db.KeyExpireAsync(key, expireTime);
                return data;
            });
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double HashDecrement(string key, string dataKey, double val = 1)
        {
            key = PrefixKey(key);
            return Do(db => db.HashDecrementAsync(key, dataKey, val)).Result;
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> HashKeys<T>(string key)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                RedisValue[] values = db.HashKeysAsync(key).Result;
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 获取hashkey所有Redis key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, T> HashGetAll<T>(string key)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                HashEntry[] values = db.HashGetAllAsync(key).Result;
                return ConvetDic<T>(values);
            });
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashExistsAsync(string key, string dataKey)
        {
            key = PrefixKey(key);
            return await Do(db => db.HashExistsAsync(key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns> 返回true表示是新加，返回false表示是修改 </returns>
        public async Task<bool> HashSetAsync<T>(string key, string dataKey, T t)
             => await Do(db => { return db.HashSetAsync(PrefixKey(key), dataKey, Serialize(t)); });

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashDeleteAsync(string key, string dataKey)
            => await Do(db => db.HashDeleteAsync(PrefixKey(key), dataKey));

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<long> HashDeleteAsync(string key, params object[] dataKey)
        {
            key = PrefixKey(key);
            return await Do(db => db.HashDeleteAsync(key, ConvertRedisValue(dataKey)));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<T> HashGetAsync<T>(string key, string dataKey)
        {
            key = PrefixKey(key);
            string value = await Do(db => db.HashGetAsync(key, dataKey));
            return Deserialize<T>(value);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <param name="expireTime">过期时间</param>
        /// <returns>增长后的值</returns>
        public async Task<double> HashIncrementAsync(string key, string dataKey, double val = 1, DateTime? expireTime = null)
        {
            key = PrefixKey(key);
            return await Do(db =>
            {
                var data = db.HashIncrementAsync(key, dataKey, val);
                if (expireTime.HasValue)
                    db.KeyExpireAsync(key, expireTime);
                return data;
            });
        }

        public async Task<double> HashIncrementAsync(string key, string dataKey, double val, TimeSpan? timeSpan)
        {
            key = PrefixKey(key);
            return await Do(db =>
            {
                var data = db.HashIncrementAsync(key, dataKey, val);
                db.KeyExpireAsync(key, timeSpan);
                return data;
            });
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public async Task<double> HashDecrementAsync(string key, string dataKey, double val = 1)
        {
            key = PrefixKey(key);
            return await Do(db => db.HashDecrementAsync(key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> HashKeysAsync<T>(string key)
        {
            key = PrefixKey(key);
            RedisValue[] values = await Do(db => db.HashKeysAsync(key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 获取hashkey所有Redis key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            key = PrefixKey(key);
            HashEntry[] values = await Do(db => db.HashGetAllAsync(key));
            return ConvetDic<T>(values);
        }

        #endregion 异步方法

        #endregion Hash

        #region Set 无序集合

        #region 同步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan">过期时间</param>
        public bool SetAdd<T>(string key, T value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.SetAddAsync(key, Serialize(value)).Result;
                if (timeSpan.HasValue)
                    db.KeyExpireAsync(key, timeSpan);
                return data;
            });
        }
        public bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.SetAddAsync(key, ConvertRedisValue(value)).Result;
                if (timeSpan.HasValue)
                    db.KeyExpireAsync(key, timeSpan);
                return data > 0;
            });
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public long SetRemove<T>(string key, params T[] value)
        {
            key = PrefixKey(key);
            RedisValue[] valueList = value.Select(v => (RedisValue)Serialize(v)).ToArray();
            return Do(redis => redis.SetRemoveAsync(key, valueList).Result);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetMembers<T>(string key)
        {
            key = PrefixKey(key);
            return Do(redis =>
            {
                var values = redis.SetMembersAsync(key).Result;
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 随机获取一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T SetRandomMember<T>(string key)
        {
            key = PrefixKey(key);
            return Do(redis =>
            {
                var value = redis.SetRandomMemberAsync(key).Result;
                return Deserialize<T>(value);
            });
        }

        /// <summary>
        /// 随机获取多个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> SetRandomMembers<T>(string key, long count = 1)
        {
            count = count < 1 ? 1 : count;
            key = PrefixKey(key);
            return Do(redis =>
            {
                var value = redis.SetRandomMembersAsync(key, count).Result.Select(x => Deserialize<T>(x)).ToList();
                return value;
            });
        }

        /// <summary>
        /// 判断key集合中是否包含指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetContains<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(redis =>
            {
                return redis.SetContainsAsync(key, Serialize(value)).Result;

            });
        }

        /// <summary>
        ///  随机删除key集合中的一个值，并返回该值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param> 
        /// <returns></returns>
        public T SetPop<T>(string key)
        {
            key = PrefixKey(key);
            return Do(redis =>
            {
                var v = redis.SetPopAsync(key).Result;
                return Deserialize<T>(v);
            });
        }

        /// <summary>
        /// 随机删除key集合中的count个值，并返回count个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> SetPop<T>(string key, long count)
        {
            count = count < 1 ? 1 : count;
            key = PrefixKey(key);
            return Do(redis =>
            {
                var list = redis.SetPopAsync(key, count).Result.Select(x => Deserialize<T>(x)).ToList();
                return list;
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SetLength(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SetLengthAsync(key)).Result;
        }


        /// <summary>
        /// 并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetUnion<T>(params string[] key)
        {
            return Do(redis =>
            {
                key = key.Select(k => PrefixKey(k)).ToArray();
                var values = redis.SetCombineAsync(SetOperation.Union, ConvertRedisKeys(key)).Result;
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetIntersect<T>(params string[] key)
        {
            return Do(redis =>
            {
                key = key.Select(k => PrefixKey(k)).ToArray();
                var values = redis.SetCombineAsync(SetOperation.Intersect, ConvertRedisKeys(key)).Result;
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetDifference<T>(params string[] key)
        {
            return Do(redis =>
            {
                key = key.Select(k => PrefixKey(k)).ToArray();
                var values = redis.SetCombineAsync(SetOperation.Difference, ConvertRedisKeys(key)).Result;
                return ConvetList<T>(values);
            });
        }
        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<bool> SetAddAsync<T>(string key, T value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            //return await Do(redis => redis.SetAddAsync(key, Serialize(value)));
            return await Do(async db =>
            {
                var data = await db.SetAddAsync(key, Serialize(value));
                if (timeSpan.HasValue)
                    await db.KeyExpireAsync(key, timeSpan);
                return data;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public async Task<bool> SetAddAsync<T>(string key, T[] value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            return await Do(async db =>
           {
               var data = await db.SetAddAsync(key, ConvertRedisValue(value));
               if (timeSpan.HasValue)
                   await db.KeyExpireAsync(key, timeSpan);
               return data > 0;
           });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> SetRemoveAsync<T>(string key, params T[] value)
        {
            key = PrefixKey(key);
            RedisValue[] valueList = value.Select(v => (RedisValue)Serialize(v)).ToArray();
            return await Do(redis => redis.SetRemoveAsync(key, valueList));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SetMembersAsync<T>(string key)
        {
            key = PrefixKey(key);
            var ret = await Do(redis =>
             {
                 return redis.SetMembersAsync(key);
             });
            return ConvetList<T>(ret);
        }

        /// <summary>
        /// 随机获取一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> SetRandomMemberAsync<T>(string key)
        {
            key = PrefixKey(key);
            var ret = await Do(redis =>
             {
                 return redis.SetRandomMemberAsync(key);
             });
            return Deserialize<T>(ret);
        }

        /// <summary>
        /// 随机获取多个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<T>> SetRandomMembersAsync<T>(string key, long count = 1)
        {
            count = count < 1 ? 1 : count;
            key = PrefixKey(key);
            var ret = await Do(redis =>
             {
                 return redis.SetRandomMembersAsync(key, count);
             });
            return ret.Select(x => Deserialize<T>(x)).ToList();
        }

        /// <summary>
        /// 判断key集合中是否包含指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> SetContainsAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return await Do(redis =>
            {
                return redis.SetContainsAsync(key, Serialize(value));
            });
        }

        /// <summary>
        ///  随机删除key集合中的一个值，并返回该值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param> 
        /// <returns></returns>
        public async Task<T> SetPopAsync<T>(string key)
        {
            key = PrefixKey(key);
            var ret = await Do(redis =>
             {
                 return redis.SetPopAsync(key);
             });
            return Deserialize<T>(ret);
        }

        /// <summary>
        /// 随机删除key集合中的count个值，并返回count个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<T>> SetPopAsync<T>(string key, long count)
        {
            count = count < 1 ? 1 : count;
            key = PrefixKey(key);
            var ret = await Do(redis =>
            {
                return redis.SetPopAsync(key, count);
            });
            return ret.Select(x => Deserialize<T>(x)).ToList();
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SetLengthAsync(string key)
        {
            key = PrefixKey(key);
            return await Do(redis => redis.SetLengthAsync(key));
        }

        #endregion 异步方法

        #endregion Set 无序集合

        #region SortedSet 有序集合

        #region 同步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public bool SortedSetAdd<T>(string key, T value, double score)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetAddAsync(key, Serialize(value), score)).Result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool SortedSetRemove<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetRemoveAsync(key, Serialize(value))).Result;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SortedSetRangeByRank<T>(string key)
        {
            key = PrefixKey(key);
            return Do(redis =>
            {
                var values = redis.SortedSetRangeByRankAsync(key).Result;
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetLengthAsync(key)).Result;
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public async Task<bool> SortedSetAddAsync<T>(string key, T value, double score)
        {
            key = PrefixKey(key);
            return await Do(redis => redis.SortedSetAddAsync(key, Serialize(value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<bool> SortedSetRemoveAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return await Do(redis => redis.SortedSetRemoveAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string key)
        {
            key = PrefixKey(key);
            var values = await Do(redis => redis.SortedSetRangeByRankAsync(key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SortedSetLengthAsync(string key)
        {
            key = PrefixKey(key);
            return await Do(redis => redis.SortedSetLengthAsync(key));
        }

        #endregion 异步方法

        #endregion SortedSet 有序集合

        #region key

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;
            return Do(db => db.KeyDeleteAsync(PrefixKey(key))).Result;
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(List<string> keys)
        {
            string[] newKeys = keys.Select(x => PrefixKey(x)).ToArray();
            return Do(db => db.KeyDeleteAsync(ConvertRedisKeys(newKeys))).Result;
        }

        /// <summary>
        /// 删除以<paramref name="pattern"/>开头的key
        /// </summary>
        /// <param name="pattern"></param>
        public async Task KeyDeleteStartWith(string pattern)
        {
            var keys = await QueryStartWith(pattern, false);
            if (keys.IsNullOrEmpty()) return;
            await Do(db => db.KeyDeleteAsync(ConvertRedisKeys(keys)));
            //string luaScript = $"redis.call('del', unpack(redis.call('keys','{pattern}*')))";

            //mock数据
            // eval "for i = 1, 100000 do redis.call('SET','mockKeys:' .. i,i) end" 0

            // 2023/03/27 del
            //StringBuilder sb = new StringBuilder()
            //      .AppendLine("local c = '0'")
            //      .AppendLine("repeat")
            //      .AppendLine($" local resp = redis.call('SCAN', c, 'MATCH', @keypattern, 'COUNT', 100)")
            //      .AppendLine(" c = resp[1]")
            //      .AppendLine(" for _, key in ipairs(resp[2]) do")
            //      .AppendLine("    local ttl = redis.call('TTL', key)")
            //      .AppendLine("    if ttl == -1 then")
            //      .AppendLine("        redis.call('DEL', key)")
            //      .AppendLine("    end")
            //      .AppendLine(" end")
            //      .AppendLine("until c =='0'")
            //      .AppendLine("return c");
            //_db.ScriptEvaluate(LuaScript.Prepare(sb.ToString()), new { keypattern = $"{PrefixKey(pattern)}*" });

        }
        /// <summary>
        /// 查询<paramref name="pattern"/>开头的keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removePrefix">默认移除实例前缀</param>
        /// <returns></returns>
        public async Task<string[]> QueryStartWith(string pattern, bool removePrefix = true)
        {
            StringBuilder sbLuaScript = new StringBuilder()
                   .AppendLine("local keys, has, cursor = {}, {},'0';")
                   .AppendLine("repeat")
                   .AppendLine(" local result = redis.call('SCAN',cursor,'MATCH', @keypattern,'COUNT', 100)")
                   .AppendLine(" cursor = result[1]")
                   .AppendLine(" for _, key in ipairs(result[2]) do")
                   .AppendLine("    if has[key] == nil then has[key] = 1; keys[#keys+1] = key;")
                   .AppendLine("   end")
                   .AppendLine(" end")
                   .AppendLine("until cursor =='0'")
                   .AppendLine("return keys");

            var result = await _db.ScriptEvaluateAsync(LuaScript.Prepare(sbLuaScript.ToString()), new { keypattern = PrefixKey(pattern) + "*" });

            if (!result.IsNull)
            {
                var arr = (string[])result;
                if (!removePrefix) return arr;

                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = arr[i].Remove(0, _options.InstanceName.Length);
                }
                return arr;
            }

            return new string[0];
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            key = PrefixKey(key);
            return Do(db => db.KeyExistsAsync(key)).Result;
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            key = PrefixKey(key);
            return Do(db => db.KeyRenameAsync(key, newKey)).Result;
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            key = PrefixKey(key);
            return Do(db => db.KeyExpireAsync(key, expiry)).Result;
        }

        public bool KeyExpire(string key, DateTime dateTime)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            key = PrefixKey(key);
            return Do(db => db.KeyExpireAsync(key, dateTime)).Result;
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void FlushDb()
        {
            FlushCurrentDatabase();
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">锁名称，不可重复</param>
        /// <param name="value">用于释放锁的标记</param>
        /// <param name="cacheSeconds">锁有效期,秒</param>
        /// <returns></returns>
        public bool LockTake(string key, string value = "", int cacheSeconds = 10)
        {
            return _db.LockTakeAsync(PrefixKey(key), value, TimeSpan.FromSeconds(cacheSeconds)).Result;
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheSeconds"></param>
        /// <returns></returns>
        public bool LockTake(string key, int cacheSeconds)
        {
            return _db.LockTakeAsync(PrefixKey(key), "", TimeSpan.FromSeconds(cacheSeconds)).Result;
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁名称</param>
        /// <param name="value">标记，必须与锁的时候一直</param>
        /// <returns></returns>
        public bool LockRelease(string key, string value = "")
        {
            return _db.LockReleaseAsync(PrefixKey(key), value).Result;
        }

        /// <summary>
        /// 根据值获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<string> GetKeys<T>(T value)
        {
            List<string> keys = new List<string>();
            string tmp = Serialize(value);

            if (value == null || string.IsNullOrWhiteSpace(tmp))
                return keys;

            var allKeys = GetAllKeys();
            foreach (var key in allKeys)
            {
                string objValue = _db.StringGetAsync(key).Result;

                if (tmp == objValue)
                {
                    keys.Add(key.TrimStart(PrefixKey("").ToCharArray()));
                }
            }
            return keys;
        }
        #endregion key

        #region 发布订阅

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        public void Subscribe(string subChannel)
        {
            Subscribe(subChannel, (c, h) =>
            {
                Console.WriteLine("通道：" + c);
                Console.WriteLine("消息内容：" + h);
            });
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public void Subscribe(string subChannel, Action<string> handler)
        {
            Subscribe(subChannel, (c, h) => { handler(h); });
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public void Subscribe<T>(string subChannel, Action<T> handler)
        {
            Subscribe(subChannel, (c, h) =>
            {
                handler(Deserialize<T>(h));
            });
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public async Task SubscribeAsync(string subChannel, Func<string, Task> handler)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            await sub.SubscribeAsync(chl, async (channel, message) => await handler?.Invoke(message));
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public async Task SubscribeAsync<T>(string subChannel, Func<T, Task> handler)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            await sub.SubscribeAsync(chl, async (channel, message)
                => await handler?.Invoke(Deserialize<T>(message)));
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public async Task SubscribeAsync<T>(string subChannel, Action<T> handler)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            await sub.SubscribeAsync(chl, (_, message)
                => handler(Deserialize<T>(message)));
        }

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        protected void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            sub.SubscribeAsync(chl, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " 订阅收到消息：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            }).GetAwaiter();
        }

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public long Publish<T>(string channel, T msg)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(channel), RedisChannel.PatternMode.Auto);
            return sub.PublishAsync(chl, Serialize(msg)).Result;
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(channel), RedisChannel.PatternMode.Auto);
            sub.UnsubscribeAsync(chl).GetAwaiter();
        }

        /// <summary>
        /// [慎重调用]Redis发布订阅  取消全部订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            ISubscriber sub = _connection.GetSubscriber();
            sub.UnsubscribeAllAsync().GetAwaiter();
        }

        #endregion 发布订阅

        #region 其他

        /// <summary>
        /// 设置前缀
        /// </summary>
        /// <param name="key"></param>
        string PrefixKey(string key)
        {
            return $"{_options.InstanceName}{key}";
        }


        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="objvalue"></param>
        /// <returns></returns>
        string Serialize(object objvalue)
        {
            if (objvalue is IConvertible)
                return objvalue.ToString();

            return JsonHelper.Serialize(objvalue);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tValue"></param>
        /// <returns></returns>
        T Deserialize<T>(string tValue)
        {
            if (tValue.IsNullOrEmpty()) return default(T);
            if (typeof(T) == typeof(string) || CheckIConvertible(default(T)))
                return ConvertHelper.ConvertTo<T>(tValue, default(T), false);

            if (JsonHelper.IsJson(tValue))
                return JsonHelper.Deserialize<T>(tValue);

            throw new FormatException(tValue);
        }

        /// <summary> 
        /// 将值反系列化成对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam> 
        /// <param name="values"></param> 
        /// <returns></returns>
        List<T> ConvetList<T>(RedisValue[] values)
        {
            List<T> list = new List<T>();
            if (values == null || values.Length <= 0)
                return list;

            list = values.Select(o => Deserialize<T>(o)).ToList();
            return list;
        }

        /// <summary>  
        /// 将值集合转换成RedisValue集合 
        /// </summary> 
        /// <typeparam name="T"></typeparam>
        /// <param name="redisValues"></param> 
        /// <returns></returns>  
        RedisValue[] ConvertRedisValue<T>(params T[] redisValues) => redisValues.Select(o => (RedisValue)Serialize(o)).ToArray();

        /// <summary>
        /// key集合转RedisKey集合
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        RedisKey[] ConvertRedisKeys(string[] redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }

        RedisKey[] ConvertRedisKeys(object[] redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }

        /// <summary>
        /// null不能判断，所以string和可空类型不能用这个
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool CheckIConvertible(object obj)
        {
            if (obj is IConvertible)
                return true;

            return false;
        }

        protected T Do<T>(Func<IDatabase, T> func)
        {
            return func(_db);
        }

        /// <summary>
        /// hash转换成Dic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected Dictionary<string, T> ConvetDic<T>(HashEntry[] values)
        {
            Dictionary<string, T> result = new Dictionary<string, T>();
            bool? isJson = null;
            foreach (var item in values)
            {
                var val = item.Value.ToString();
                if (!isJson.HasValue)
                    isJson = JsonHelper.IsJson(val);

                var model = isJson.Value ? JsonHelper.Deserialize<T>(val) : ConvertHelper.ConvertTo<T>(val);
                result.Add(item.Name, model);
            }
            return result;
        }

        #endregion 其他

    }
}
