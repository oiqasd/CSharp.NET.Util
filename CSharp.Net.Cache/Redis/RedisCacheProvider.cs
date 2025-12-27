using CSharp.Net.Cache.Redis;
using CSharp.Net.Util;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Cache
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
        public T GetOrSet<T>(string key, Func<Task<T>> func, TimeSpan expiry) //where T : new()
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
                    return value;

                value = func().Result;
                if (value != null)
                {
                    StringSet(key, value, (int)expiry.TotalSeconds);
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
        public T GetOrSet<T>(string key, Func<Task<T>> func, int seconds = 0)
        {
            if (_db.KeyExists(PrefixKey(key)))
                return StringGet<T>(key);

            lock (_lockObj)
            {
                var data = func().Result;
                StringSet(key, data, seconds);
                return data;
            }
        }

        /// <summary>
        /// 获取或添加key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public T GetOrSet<T>(string key, Func<T> func, int seconds = 0)
        {
            if (_db.KeyExists(PrefixKey(key)))
                return StringGet<T>(key);

            //if (LockTake("lck_" + key))
            lock (_lockObj)
            {
                var data = func.Invoke();
                StringSet(key, data, seconds);
                return data;
            }
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
            return _db.StringSet(key, value, expiry);
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

            /*
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
            keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(PrefixKey(p.Key), p.Value)).ToList();
            return Do(db => db.StringSet(newkeyValues.ToArray()));
             */

            var batch = _db.CreateBatch();
            Parallel.ForEach(keyValues, str =>
            {
                if (str.Value == null) return;
                batch.StringSetAsync(str.Key, str.Value, cacheSeconds > 0 ? TimeSpan.FromSeconds(cacheSeconds) : (TimeSpan?)null);
            });
            batch.Execute();

            return true;
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
            return _db.StringGet(key);
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
                double data = db.StringIncrement(pkey, val);
                if (expireTimeSpan != null && (updateExpire || data <= val))
                    db.KeyExpire(pkey, expireTimeSpan);

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
                var data = db.StringIncrement(pkey, val);
                if (expireTime.HasValue)
                    db.KeyExpire(pkey, expireTime.Value);
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
            return _db.StringDecrement(key, val);
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 批量读
        /// </summary>
        public async Task<Dictionary<string, string>> StringGetAsync(string[] keys)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            if (keys == null || keys.Count() <= 0)
                throw new ArgumentNullException(nameof(keys));

            ConcurrentDictionary<string, string> redisValueList = new ConcurrentDictionary<string, string>();
            var batch = _db.CreateBatch();
#if NET
            await Parallel.ForEachAsync(keys.Distinct(), async (str, t) =>
             {
                 var v = await batch.StringGetAsync(PrefixKey(str));
                 redisValueList.TryAdd(str, v);
             });
#else
            Parallel.ForEach(keys.Distinct(), str =>
            {
                var v = batch.StringGetAsync(PrefixKey(str));
                redisValueList.TryAdd(str, v.Result);
            });
#endif
            batch.Execute();
            foreach (var k in redisValueList) data.Add(k.Key, k.Value);
            return data;
        }


        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> StringGetAsync<T>(string key)
        {
            var data = await StringGetAsync(key);
            return Deserialize<T>(data);
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public async Task<string> StringGetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            key = PrefixKey(key);
            return await _db.StringGetAsync(key);
        }

        /// <summary>
        /// 保存单个key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheSeconds"></param>
        public Task<bool> StringSetAsync(string key, string value, int cacheSeconds = 0)
        {
            TimeSpan? timeSpan = null;
            if (cacheSeconds > 0)
                timeSpan = TimeSpan.FromSeconds(cacheSeconds);

            return StringSetAsync(key, value, timeSpan);
        }

        /// <summary>
        /// 保存单个key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry)
        {
            Args.Verify(key.IsNotNullOrEmpty());
            key = PrefixKey(key);
            return _db.StringSetAsync(key, value, expiry);
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public Task<bool> StringSetAsync<T>(string key, T obj, TimeSpan? expiry) where T : new()
        => StringSetAsync(key, Serialize(obj), expiry);


        /// <summary>
        /// 递增
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="val">value</param>
        /// <param name="expireTimeSpan">过期时间</param>
        /// <param name="updateExpire">是否更新过期时间,滑动过期</param>
        /// <returns></returns>
        public Task<double> StringIncrementAsync(string key, double val = 1, TimeSpan? expireTimeSpan = null, bool updateExpire = false)
        {
            var pkey = PrefixKey(key);
            var ret = Do(db =>
            {
                var data = db.StringIncrementAsync(pkey, val);
                if (expireTimeSpan != null && updateExpire)
                    data.ContinueWith(_ => db.KeyExpireAsync(pkey, expireTimeSpan));
                return data;
            });
            return ret;
        }

        /// <summary>
        /// 递减
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public Task<double> StringDecrementAsync(string key, double val = 1)
        {
            key = PrefixKey(key);
            return Do(db => db.StringDecrementAsync(key, val));
        }
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
            Do(db => db.ListRemove(key, Serialize(value)));
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
                var values = redis.ListRange(key);
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
            Do(db => db.ListRightPush(key, Serialize(value)));
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
            var value = _db.ListRightPop(key);
            return Deserialize<T>(value);
        }
        public List<T> ListRightPop<T>(string key, int count)
        {
            key = PrefixKey(key);
            var value = _db.ListRightPop(key, count);
            return ConvetList<T>(value);
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
            _db.ListLeftPush(key, Serialize(value));
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
            var value = _db.ListLeftPop(key);
            return Deserialize<T>(value);
        }
        /// <summary>
        ///  version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> ListLeftPop<T>(string key, int count)
        {
            key = PrefixKey(key);
            var value = _db.ListLeftPop(key, count);
            return ConvetList<T>(value);
        }
        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.ListLength(key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<long> ListRemoveAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(db => db.ListRemoveAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <remarks>This method retrieves elements from a Redis list within the specified range. The
        /// range is inclusive,  meaning both the start and stop indices are included. If the specified range exceeds
        /// the bounds of  the list, only the elements within the valid range are returned.</remarks>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="key">The key identifying the list in Redis. Cannot be null or empty.</param>
        /// <param name="start">The zero-based index of the first element to retrieve. Defaults to 0.</param>
        /// <param name="stop">The zero-based index of the last element to retrieve. A value of -1 retrieves all elements  from the start
        /// index to the end of the list. Defaults to -1.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of elements  of type
        /// <typeparamref name="T"/> retrieved from the specified range of the list. If the range  is empty, an empty
        /// list is returned.</returns>
        public async Task<List<T>> ListRangeAsync<T>(string key, int start = 0, int stop = -1)
        {
            key = PrefixKey(key);
            var values = await Do(redis => redis.ListRangeAsync(key, start, stop));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(db => db.ListRightPushAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 出栈
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
        /// Removes and returns a specified number of elements from the end of a list stored in the database.
        /// </summary>
        /// <remarks>This method interacts with the database to retrieve and remove elements from the
        /// specified list. If the list contains fewer elements than the specified <paramref name="count"/>, all
        /// available elements will be returned.</remarks>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="key">The key identifying the list in the database. Cannot be null or empty.</param>
        /// <param name="count">The number of elements to remove and return from the end of the list. Must be greater than zero.</param>
        /// <returns>A list of elements of type <typeparamref name="T"/> removed from the end of the list. The list will be empty
        /// if the key does not exist or the list is empty.</returns>
        public async Task<List<T>> ListRightPopAsync<T>(string key, int count)
        {
            key = PrefixKey(key);
            var ts = await Do(db => db.ListRightPopAsync(key, count));

            return ConvetList<T>(ts);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(db => db.ListLeftPushAsync(key, Serialize(value)));
        }

        /// <summary>
        /// Removes and returns the first element from the list stored at the specified key.
        /// </summary>
        /// <remarks>This method interacts with a data store to retrieve and remove the first element of
        /// the list. The key is automatically prefixed before accessing the data store.</remarks>
        /// <typeparam name="T">The type of the element to be returned.</typeparam>
        /// <param name="key">The key identifying the list. The key cannot be null or empty.</param>
        /// <returns>The first element of the list, deserialized to the specified type <typeparamref name="T"/>. Returns the
        /// default value of <typeparamref name="T"/> if the list is empty or the key does not exist.</returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            key = PrefixKey(key);
            var value = await Do(db => db.ListLeftPopAsync(key));
            return Deserialize<T>(value);
        }

        /// <summary>
        /// Removes and returns the leftmost element(s) from a list stored at the specified key.
        /// </summary>
        /// <remarks>This method interacts with the underlying data store to retrieve and deserialize the
        /// elements. The operation is asynchronous and may involve network or I/O latency.</remarks>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="key">The key identifying the list in the data store. Cannot be null or empty.</param>
        /// <param name="count">The maximum number of elements to remove and return. Defaults to 10,000.</param>
        /// <returns>The leftmost element(s) from the list, deserialized to the specified type <typeparamref name="T"/>. If the
        /// list is empty or the key does not exist, returns the default value of <typeparamref name="T"/>.</returns>
        public async Task<List<T>> ListLeftPopAsync<T>(string key, int count)
        {
            key = PrefixKey(key);
            var value = await Do(db => db.ListLeftPopAsync(key, count));
            return ConvetList<T>(value);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<long> ListLengthAsync(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.ListLengthAsync(key));
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
            return Do(db => db.HashExists(key, dataKey));
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
                var data = db.HashSet(key, dataKey, Serialize(t));
                if (expireTime != null)
                {
                    if (expireTime.Value <= DateTime.Now)
                        throw new ArgumentException("Expire time can't less now");

                    db.KeyExpire(key, expireTime.Value);
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
                var data = db.HashSet(key, dataKey, Serialize(t));
                db.KeyExpire(key, expireTimeSpan);
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
            return Do(db => db.HashDelete(key, dataKey));
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
            return Do(db => db.HashDelete(key, ConvertRedisValue(dataKeys)));
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
                string value = db.HashGet(key, dataKey);
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
                var data = db.HashIncrement(key, dataKey, val);
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
                var data = db.HashIncrement(key, dataKey, val);
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
            return Do(db => db.HashDecrement(key, dataKey, val));
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
                RedisValue[] values = db.HashKeys(key);
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
                HashEntry[] values = db.HashGetAll(key);
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
        public Task<bool> HashExistsAsync(string key, string dataKey)
        {
            key = PrefixKey(key);
            return Do(db => db.HashExistsAsync(key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="expired"></param>
        /// <returns> 返回true表示是新加，返回false表示是修改 </returns>
        public Task<bool> HashSetAsync<T>(string key, string dataKey, T t, TimeSpan? expired = null)
        {
            return Do(db =>
            {
                var k = db.HashSetAsync(PrefixKey(key), dataKey, Serialize(t));
                k.ContinueWith(_ => { if (expired.HasValue) db.KeyExpireAsync(key, expired); });
                return k;
            });
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public Task<bool> HashDeleteAsync(string key, string dataKey)
            => Do(db => db.HashDeleteAsync(PrefixKey(key), dataKey));

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public Task<long> HashDeleteAsync(string key, params object[] dataKey)
            => Do(db => db.HashDeleteAsync(PrefixKey(key), ConvertRedisValue(dataKey)));

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
        /// Increments the value of a specified field in a hash stored in Redis by a given amount.
        /// </summary>
        /// <remarks>If the specified field does not exist, it will be created and initialized to the
        /// value of <paramref name="val"/>. If the hash does not exist, it will be created.</remarks>
        /// <param name="key">The key of the hash in Redis. Cannot be null or empty.</param>
        /// <param name="dataKey">The field within the hash to increment. Cannot be null or empty.</param>
        /// <param name="val">The amount by which to increment the field's value. Can be positive or negative.</param>
        /// <param name="expired">An optional expiration time for the hash. If specified, the hash's time-to-live will be updated after the
        /// increment operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the new value of the field after
        /// the increment operation.</returns>
        public Task<double> HashIncrementAsync(string key, string dataKey, double val = 1, TimeSpan? expired = null)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var tk = db.HashIncrementAsync(key, dataKey, val);
                tk.ContinueWith(_ => db.KeyExpireAsync(key, expired));
                return tk;
            });
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public Task<double> HashDecrementAsync(string key, string dataKey, double val = 1)
        {
            key = PrefixKey(key);
            return Do(db => db.HashDecrementAsync(key, dataKey, val));
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
                var data = db.SetAdd(key, Serialize(value));
                if (timeSpan.HasValue)
                    db.KeyExpire(key, timeSpan);
                return data;
            });
        }
        public bool SetAdd<T>(string key, List<T> value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            return Do(db =>
            {
                var data = db.SetAdd(key, ConvertRedisValue(value.ToArray()));
                if (timeSpan.HasValue)
                    db.KeyExpire(key, timeSpan);
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
            return Do(redis => redis.SetRemove(key, valueList));
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
                var values = redis.SetMembers(key);
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
                var value = redis.SetRandomMember(key);
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
                var value = redis.SetRandomMembers(key, count).Select(x => Deserialize<T>(x)).ToList();
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
                return redis.SetContains(key, Serialize(value));

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
                var v = redis.SetPop(key);
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
                var list = redis.SetPop(key, count).Select(x => Deserialize<T>(x)).ToList();
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
            return Do(redis => redis.SetLength(key));
        }



        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        public Task<bool> SetAddAsync<T>(string key, T value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            //return await Do(redis => redis.SetAddAsync(key, Serialize(value)));
            return Do(db =>
            {
                var data = db.SetAddAsync(key, Serialize(value));
                data.ContinueWith(x => db.KeyExpireAsync(key, timeSpan));
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
        public Task<long> SetAddAsync<T>(string key, T[] value, TimeSpan? timeSpan = null)
        {
            key = PrefixKey(key);
            return Do(db =>
           {
               var data = db.SetAddAsync(key, ConvertRedisValue(value));
               data.ContinueWith(x => db.KeyExpireAsync(key, timeSpan));
               return data;
           });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<long> SetRemoveAsync<T>(string key, params T[] value)
        {
            key = PrefixKey(key);
            RedisValue[] valueList = value.Select(v => (RedisValue)Serialize(v)).ToArray();
            return Do(redis => redis.SetRemoveAsync(key, valueList));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SetMembersAsync<T>(string key)
        {
            key = PrefixKey(key);
            var ret = await Do(redis => { return redis.SetMembersAsync(key); });
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
            var ret = await Do(redis => { return redis.SetRandomMemberAsync(key); });
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
            Args.Verify(count >= 1);
            key = PrefixKey(key);
            var ret = await Do(redis => { return redis.SetRandomMembersAsync(key, count); });
            return ret.Select(x => Deserialize<T>(x)).ToList();
        }

        /// <summary>
        /// 判断key集合中是否包含指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<bool> SetContainsAsync<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(redis => { return redis.SetContainsAsync(key, Serialize(value)); });
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
            var ret = await Do(redis => { return redis.SetPopAsync(key); });
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
            var ret = await Do(redis => { return redis.SetPopAsync(key, count); });
            return ret.Select(x => Deserialize<T>(x)).ToList();
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<long> SetLengthAsync(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SetLengthAsync(key));
        }

        /// <summary>
        /// 并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SetUnionAsync<T>(params string[] key)
        {
            var values = await Do(redis =>
            {
                key = key.Select(k => PrefixKey(k)).ToArray();
                return redis.SetCombineAsync(SetOperation.Union, ConvertRedisKeys(key));
            });
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SetIntersectAsync<T>(params string[] key)
        {
            var values = await Do(redis =>
            {
                key = key.Select(k => PrefixKey(k)).ToArray();
                return redis.SetCombineAsync(SetOperation.Intersect, ConvertRedisKeys(key));
            });
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SetDifferenceAsync<T>(params string[] key)
        {
            var values = await Do(redis =>
            {
                key = key.Select(k => PrefixKey(k)).ToArray();
                return redis.SetCombineAsync(SetOperation.Difference, ConvertRedisKeys(key));
            });
            return ConvetList<T>(values);
        }
        #endregion 异步方法

        #endregion Set 无序集合

        #region SortedSet 有序集合

        #region 同步方法
        /*
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
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetLengthAsync(key)).Result;
        }
        */
        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public Task<bool> SortedSetAdd<T>(string key, T value, double score)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetAddAsync(key, Serialize(value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Task<bool> SortedSetRemove<T>(string key, T value)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetRemoveAsync(key, Serialize(value)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="sortAsc"></param>
        /// <returns></returns>
        public async Task<List<T>> SortedSetRangeByRank<T>(string key, long start = 0, long stop = -1, bool sortAsc = true)
        {
            key = PrefixKey(key);
            var values = await Do(redis => redis.SortedSetRangeByRankAsync(key, start, stop, sortAsc ? Order.Ascending : Order.Descending));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<long> SortedSetLength(string key)
        {
            key = PrefixKey(key);
            return Do(redis => redis.SortedSetLengthAsync(key));
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
            Args.Verify(key.IsNotNullOrEmpty());
            return Do(db => db.KeyDelete(PrefixKey(key)));
        }
        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public Task<bool> KeyDeleteAsync(string key)
        {
            Args.Verify(key.IsNotNullOrEmpty());
            return Do(db => db.KeyDeleteAsync(PrefixKey(key)));
        }
        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public Task<long> KeyDeleteAsync(List<string> keys)
        {
            string[] newKeys = keys.Select(x => PrefixKey(x)).ToArray();
            return Do(db => db.KeyDeleteAsync(ConvertRedisKeys(newKeys)));
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
            return Do(db => db.KeyExists(key));
        }
        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<bool> KeyExistsAsync(string key)
        {
            key = PrefixKey(key);
            return Do(db => db.KeyExistsAsync(key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public Task<bool> KeyRenameAsync(string key, string newKey)
        {
            key = PrefixKey(key);
            return Do(db => db.KeyRenameAsync(key, newKey));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            key = PrefixKey(key);
            return Do(db => db.KeyExpire(key, expiry));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public Task<bool> KeyExpireAsync(string key, TimeSpan? expiry = null)
        {
            key = PrefixKey(key);
            return Do(db => db.KeyExpireAsync(key, expiry));
        }

        /// <summary>
        /// 获取key的过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<DateTime?> KeyExpireTimeAsync(string key)
        {
            key = PrefixKey(key);
            return Do(db => db.KeyExpireTimeAsync(key));
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
        /// <param name="key"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool LockTake(string key, int cacheSeconds = 10, string value = "")
        {
            return _db.LockTake(PrefixKey(key), "", TimeSpan.FromSeconds(cacheSeconds));
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task<bool> LockTakeAsync(string key, int cacheSeconds = 10, string value = "")
        {
            return _db.LockTakeAsync(PrefixKey(key), value, TimeSpan.FromSeconds(cacheSeconds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="waitSeconds"></param>
        /// <returns></returns>
        public void LockWait(string key, int waitSeconds = 10)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource(waitSeconds * 1000);
                var t = Task.Run(async () =>
                {
                    while (!LockTake(key, waitSeconds))
                    {
                        await Task.Delay(1);
                        // cts.Token.ThrowIfCancellationRequested();
                    }
                });
                t.Wait(cts.Token);
            }
            catch
            {
                throw new Exception("LockWait is timeout");
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁名称</param>
        /// <param name="value">标记，必须与锁的时候一直</param>
        /// <returns></returns>
        public bool LockRelease(string key, string value = "")
        {
            return _db.LockRelease(PrefixKey(key), value);
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁名称</param>
        /// <param name="value">标记，必须与锁的时候一直</param>
        /// <returns></returns>
        public Task<bool> LockReleaseAsync(string key, string value = "")
        {
            return _db.LockReleaseAsync(PrefixKey(key), value);
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
                string objValue = _db.StringGet(key);

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
        public Task SubscribeAsync(string subChannel, Func<string, Task> handler)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            return sub.SubscribeAsync(chl, (channel, message) => handler?.Invoke(message));
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public Task SubscribeAsync<T>(string subChannel, Func<T, Task> handler)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            return sub.SubscribeAsync(chl, (channel, message)
               => handler?.Invoke(Deserialize<T>(message)));
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public Task SubscribeAsync<T>(string subChannel, Action<T> handler)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(subChannel), RedisChannel.PatternMode.Auto);
            return sub.SubscribeAsync(chl, (channel, message)
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
            sub.Subscribe(chl, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " 订阅收到消息：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            });
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
            return sub.Publish(chl, Serialize(msg));
        }
        /// <summary>
        /// 发布订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Task<long> PublishAsync<T>(string channel, T msg)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(channel), RedisChannel.PatternMode.Auto);
            return sub.PublishAsync(chl, Serialize(msg));
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public Task Unsubscribe(string channel)
        {
            ISubscriber sub = _connection.GetSubscriber();
            var chl = new RedisChannel(PrefixKey(channel), RedisChannel.PatternMode.Auto);
            return sub.UnsubscribeAsync(chl);
        }

        /// <summary>
        /// [慎重调用]Redis发布订阅  取消全部订阅
        /// </summary>
        public Task UnsubscribeAll()
        {
            ISubscriber sub = _connection.GetSubscriber();
            return sub.UnsubscribeAllAsync();
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
                return ConvertHelper.ConvertTo<T>(tValue, default(T));

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
            if (values == null || values.Length <= 0 || values[0].ToString() == "[]")
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
        RedisValue[] ConvertRedisValue<T>(params T[] redisValues)
        {
            var l = redisValues.Select(o => (RedisValue)Serialize(o)).ToArray();
            return l;
        }

        /// <summary>
        /// key集合转RedisKey集合
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        //RedisKey[] ConvertRedisKeys(string[] redisKeys)
        //{
        //    return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        //}

        RedisKey[] ConvertRedisKeys(string[] redisKeys)
        {
            return redisKeys.Select(redisKey => new RedisKey(redisKey)).ToArray();
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
