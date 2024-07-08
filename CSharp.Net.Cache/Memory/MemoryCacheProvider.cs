using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharp.Net.Util;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CSharp.Net.Cache.Memory
{
    public class MemoryCacheProvider : IMemoryCache
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 102400 });

        /// <summary>
        /// 读取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key)
        {
            object objValue = null;
            if (!string.IsNullOrEmpty(key) && _cache.TryGetValue(key, out objValue))
            {
                return (T)objValue;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 根据Key索引值，获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            return _cache.Get(key);
        }

        /// <summary>
        /// 获取或添加key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public T GetOrSet<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null) //where T : new()
        {
            return _cache.GetOrCreate<T>(key, c =>
            {
                var data = func().Result;
                //c.SetOptions(new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds)));
                c.AbsoluteExpiration = TimeSpanToTimeOffset(expiry);
                c.SetValue(data);
                return data;
            });
        }
        DateTimeOffset? TimeSpanToTimeOffset(TimeSpan? expiry)
        {
            if (!expiry.HasValue)
                return null;
            return DateTimeOffset.Now.AddSeconds(expiry.Value.TotalSeconds);
        }
        /// <summary>
        /// 获取或创建一个key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="func">不存在则使用该值创建</param>
        /// <param name="seconds">默认0不过期</param>
        /// <returns></returns>
        public T GetOrSet<T>(string key, Func<T> func, int seconds = 0)
        {
            return _cache.GetOrCreate<T>(key, c =>
            {
                //c.SetOptions(new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds)));
                if (seconds > 0) c.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds);
                c.SetValue(func.Invoke());
                return func.Invoke();
            });
        }

        /// <summary>
        /// 增加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheSeconds">秒,默认0不过期</param>
        /// <param name="isSliding">是否可调过期时间 自动延长/绝对时间</param>
        /// <returns></returns>
        public void Add(string key, object data, int cacheSeconds = 0, bool isSliding = true)
        {
            if (data == null || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(Add));
            }

            MemoryCacheEntryOptions options = null;
            if (cacheSeconds > 0)
                options = isSliding ? new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(cacheSeconds)) : new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(cacheSeconds));

            _cache.Set(key, data, options);
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            object objValue = null;
            return !string.IsNullOrEmpty(key) && _cache.TryGetValue(key, out objValue);
        }

        /// <summary>
        /// 获取缓存总数据项的个数
        /// </summary>
        public int Count { get { return (int)(_cache.Count); } }

        /// <summary>
        /// 根据键值返回缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get { return _cache.Get(key); }
            set { Add(key, value); }
        }

        /// <summary>
        /// 单个清除
        /// </summary>
        /// <param name="key">/key</param>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <summary>
        /// 清除全部数据
        /// </summary>
        public void RemoveAll()
        {
            var allKeys = GetCacheKeys();
            foreach (var key in allKeys)
            {
                Remove(key);
            }
        }

        /// <summary>
        /// 通过值获取所有的Key,
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<string> GetKeys<T>(T value)
        {
            List<string> keys = new List<string>();
            var allKeys = GetCacheKeys();
            foreach (var key in allKeys)
            {
                object objValue = null;
                if (!string.IsNullOrEmpty(key) && _cache.TryGetValue(key, out objValue))
                {
                    if (objValue.GetType() == typeof(T))
                    {
                        var cacheValue = (T)objValue;
                        if (cacheValue.Equals(value))
                        {
                            keys.Add(key);
                        }
                    }
                }
            }
            return keys;
        }

        public bool StringSet(string key, string value, int cacheSeconds = 0, bool isSliding = true)
        {
            Add(key, value, cacheSeconds, isSliding);
            return true;
        }

        public bool StringSet(string key, object obj, int cacheSeconds = 0)
        {
            Add(key, obj, cacheSeconds);
            return true;
        }

        public string StringGet(string key)
        {
            return Get(key)?.ToString();
        }

        public bool KeyDelete(string key)
        {
            Remove(key);
            return true;
        }

        /// <summary>
        /// 匹配删除
        /// </summary>
        /// <param name="pattern"></param>
        public async Task KeyDeleteStartWith(string pattern)
        {
            IList<string> keys = SearchCachePre(x => x.StartsWith(pattern));
            foreach (var s in keys)
            {
                Remove(s);
            }
        }

        public bool KeyExists(string key)
        {
            return Contains(key);
        }

        public void FlushDb()
        {
            RemoveAll();
        }

        /// <summary>
        /// 匹配查询
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removePrefix"></param>
        /// <returns></returns>
        public async Task<string[]> QueryStartWith(string pattern, bool removePrefix = true)
        {
            IList<string> keys = SearchCachePre(x => x.StartsWith(pattern));
            if (keys.IsNullOrEmpty()) return null;
            return keys.ToArray();
        }

        public bool SetAdd<T>(string key, T value, TimeSpan? timeSpan = null)
        {
            if (value == null) return false;
            var data = _cache.Get<List<T>>(key) ?? new List<T>();
            data.Add(value);
            var options = timeSpan.HasValue ? new MemoryCacheEntryOptions().SetAbsoluteExpiration(timeSpan.Value) : null;
            _cache.Set(key, data, options);
            return true;
        }
        public bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan)
        {
            if (value.IsNullOrEmpty()) return false;
            var data = _cache.Get<List<T>>(key) ?? new List<T>();
            data.AddRange(value);
            var options = timeSpan.HasValue ? new MemoryCacheEntryOptions().SetAbsoluteExpiration(timeSpan.Value) : null;
            _cache.Set(key, data, options);
            return true;
        }
        public long SetRemove<T>(string key, params T[] value)
        {
            if (value.IsNullOrEmpty()) return 0;
            var data = _cache.Get<List<T>>(key);
            if (data.IsNullOrEmpty()) return 0;

            var newdata = data.Where(x => !value.Contains(x)).ToList();
            _cache.Set(key, newdata);
            return data.Count - newdata.Count;
        }

        public List<T> SetMembers<T>(string key)
        {
            var data = _cache.Get<List<T>>(key);
            return data;
        }

        public T SetRandomMember<T>(string key)
        {
            var data = _cache.Get<List<T>>(key);
            if (data.IsNullOrEmpty()) return default(T);

            return data[Utils.GetRandom(0, data.Count)];
        }

        public List<T> SetRandomMembers<T>(string key, long count = 1)
        {
            var data = _cache.Get<List<T>>(key);
            if (data.IsNullOrEmpty()) return null;
            if (count < 1) count = 1;
            if (count >= data.Count) return data;

            if ((data.Count / count) <= 10)
            {
                data = Utils.GetRandomList(data).Take((int)count).ToList();
                return data;
            }

            return GetRandomMembers(data, (int)count).ToList();
        }
        IEnumerable<T> GetRandomMembers<T>(List<T> data, int count)
        {
            int[] tmps = new int[count];
            for (int i = 0; i < count; i++)
            {
                var r = Utils.GetRandom(0, data.Count) + 1;
                if (tmps.Contains(r))
                {
                    count--;
                    continue;
                }
                tmps[i] = r;
                yield return data[r - 1];
            }
        }

        public bool SetContains<T>(string key, T value)
        {
            var data = _cache.Get<List<T>>(key);
            if (data.IsNullOrEmpty()) return false;
            if (data.Contains(value)) return true;
            return false;
        }

        #region private

        /// <summary>
        /// 搜索匹配到的缓存
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IList<string> SearchCachePre(Func<string, bool> predicate)
        {
            var cacheKeys = GetCacheKeys();
            var l = cacheKeys.Where(predicate).ToList();
            return l.AsReadOnly();
        }

        /// <summary>
        /// 搜索匹配到的缓存
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public IList<string> SearchCacheRegex(string pattern)
        {
            var cacheKeys = GetCacheKeys();
            var l = cacheKeys.Where(k => Regex.IsMatch(k, pattern)).ToList();
            return l.AsReadOnly();
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns></returns>
        List<string> GetCacheKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _cache.GetType().GetField("_entries", flags).GetValue(_cache);
            var cacheItems = entries as IDictionary;
            var keys = new List<string>();
            if (cacheItems == null) return keys;
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(cacheItem.Key.ToString());
            }
            return keys;
        }
        #endregion
    }
}

