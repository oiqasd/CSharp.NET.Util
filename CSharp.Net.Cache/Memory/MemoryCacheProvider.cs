using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

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
        /// 获取或创建一个key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">不存在则使用该值创建</param>
        /// <returns></returns>
        public string GetOrCreate(string key, string defaultValue, int seconds = 30)
        {
            return _cache.GetOrCreate<string>(key, c =>
            {
                //c.SetOptions(new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds)));
                c.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds);
                c.SetValue(defaultValue);
                return defaultValue;
            });
        }

        /// <summary>
        /// 增加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheSeconds">秒</param>
        /// <param name="isSliding">是否可调过期时间 自动延长/绝对时间</param>
        /// <returns></returns>
        public void Add(string key, object data, int cacheSeconds = 0, bool isSliding = true)
        {
            if (data == null || string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(Add));
            }

            if (cacheSeconds <= 0) cacheSeconds = 30;
            var options = isSliding ? new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(cacheSeconds)) : new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(cacheSeconds));

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
        /// 删除匹配到的缓存
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public void RemoveStartWith(string pattern)
        {
            IList<string> keys = SearchCachePre(x => x.StartsWith(pattern));
            foreach (var s in keys)
            {
                Remove(s);
            }
        }

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
        private List<string> GetCacheKeys()
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

        public bool StringSet<T>(string key, T obj, int cacheSeconds = 0, bool isSliding = true) where T : new()
        {
            Add(key, obj, cacheSeconds, isSliding);
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

        public void KeyDeleteStartWith(string pattern)
        {
            RemoveStartWith(pattern);
        }

        public bool KeyExists(string key)
        {
            return Contains(key);
        }

        public void FlushDb()
        {
            RemoveAll();
        }

        public string[] QueryStartWith(string pattern, bool removePrefix = true)
        {
            throw new NotImplementedException();
        }

        public bool SetAdd<T>(string key, T value, TimeSpan? timeSpan)
        {
            throw new NotImplementedException();
        }
        public bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan)
        {
            throw new NotImplementedException();
        }
        public long SetRemove<T>(string key, params T[] value)
        {
            throw new NotImplementedException();
        }

        public List<T> SetMembers<T>(string key)
        {
            throw new NotImplementedException();
        }

        public T SetRandomMember<T>(string key)
        {
            throw new NotImplementedException();
        }

        public List<T> SetRandomMembers<T>(string key, long count = 1)
        {
            throw new NotImplementedException();
        }

        public bool SetContains<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public T SetPop<T>(string key)
        {
            throw new NotImplementedException();
        }

        public List<T> SetPop<T>(string key, long count)
        {
            throw new NotImplementedException();
        }

        public long SetLength(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetAddAsync<T>(string key, T value, TimeSpan? timeSpan = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetAddAsync<T>(string key, T[] value, TimeSpan? timeSpan = null)
        {
            throw new NotImplementedException();
        }

        public Task<long> SetRemoveAsync<T>(string key, params T[] value)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SetMembersAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> SetRandomMemberAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SetRandomMembersAsync<T>(string key, long count = 1)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetContainsAsync<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public Task<T> SetPopAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SetPopAsync<T>(string key, long count)
        {
            throw new NotImplementedException();
        }

        public Task<long> SetLengthAsync(string key)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string subChannel, Action<string> action)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string subChannel)
        {
            throw new NotImplementedException();
        }

        public long Publish<T>(string channel, T msg)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string channel)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeAll()
        {
            throw new NotImplementedException();
        }
    }
}

