using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharp.Net.Cache.Redis
{
    public interface IRedisCache : ICache
    {
        /// <summary>
        /// 根据Key索引值，获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //object this[string key] { get; set; }

        /// <summary>
        /// 获取缓存总数据项的个数
        /// </summary>
        //int Count { get; }

        #region String
        /// <summary>
        /// 保存单个key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="isSliding"></param>
        //bool StringSet(string key, string value, int cacheSeconds = 0, bool isSliding = true);
        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        bool StringSet(string key, string value, TimeSpan? expiry);
        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        bool StringSet<T>(string key, T obj, TimeSpan? expiry) where T : new();
        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        //bool StringSet<T>(string key, T obj, int cacheSeconds = 0) where T : new();
        /// <summary>
        /// 批量添加 无过期时间
        /// </summary>
        bool StringSet(Dictionary<string, string> keyValues);
        /// <summary>
        /// 批量添加 有过期时间
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="cacheSeconds"></param>
        /// <returns></returns>
        bool StringSet(Dictionary<string, string> keyValues, int cacheSeconds);

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        //T StringGet<T>(string key);
        /// <summary>
        /// 获取key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //string StringGet(string key);
        /// <summary>
        /// 批量读
        /// </summary>
        Dictionary<string, string> StringGet(List<string> keys);

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="val">value</param>
        /// <param name="expireTimeSpan">过期时间</param>
        /// <param name="updateExpire">是否更新过期时间</param>
        /// <returns></returns>
        double StringIncrement(string key, double val = 1, TimeSpan? expireTimeSpan = null, bool updateExpire = false);
        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="val">value</param>
        /// <param name="expireTime">绝对过期时间</param>
        /// <returns></returns>
        double StringIncrement(string key, double val, DateTime? expireTime);
        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        double StringDecrement(string key, double val = 1);
        #endregion

        #region List

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void ListRemove<T>(string key, T value);
        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> ListRange<T>(string key);
        /// <summary>
        /// 右入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void ListRightPush<T>(string key, T value);
        /// <summary>
        /// 右出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T ListRightPop<T>(string key);
        /// <summary>
        /// 右出  version 6.2.0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> ListRightPop<T>(string key, int count);
        /// <summary>
        /// 左入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void ListLeftPush<T>(string key, T value);
        /// <summary>
        /// 左出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T ListLeftPop<T>(string key);
        /// <summary>
        /// 左出  version 6.2.0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> ListLeftPop<T>(string key, int count);
        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long ListLength(string key);

        #endregion

        #region Hash

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        bool HashExists(string key, string dataKey);
        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="expireTime">过期时间</param>
        /// <returns> 返回true表示是新加，返回false表示是修改</returns>
        bool HashSet<T>(string key, string dataKey, T t, DateTime? expireTime = null);
        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="expireTimeSpan">过期时间</param>
        /// <returns></returns>
        bool HashSet<T>(string key, string dataKey, T t, TimeSpan? expireTimeSpan);
        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        bool HashDelete(string key, string dataKey);
        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        long HashDelete(string key, params object[] dataKeys);

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        T HashGet<T>(string key, string dataKey);
        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> HashKeys<T>(string key);
        /// <summary>
        /// 获取hashkey所有Redis key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Dictionary<string, T> HashGetAll<T>(string key);
        /// <summary>
        /// hash自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        double HashIncrement(string key, string dataKey, double val = 1, DateTime? expireTime = null);
        /// <summary>
        /// hash自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        double HashIncrement(string key, string dataKey, double val, TimeSpan timeSpan);

        /// <summary>
        /// 异步自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <param name="expireTime"></param>
        /// <returns></returns>
        Task<double> HashIncrementAsync(string key, string dataKey, double val = 1, DateTime? expireTime = null);
        /// <summary>
        /// 异步自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        Task<double> HashIncrementAsync(string key, string dataKey, double val, TimeSpan? timeSpan);
        /// <summary>
        /// 异步自减
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        Task<double> HashDecrementAsync(string key, string dataKey, double val = 1);
        /// <summary>
        /// 获取hashkey所有Redis key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);
        #endregion

        #region Sort

        #endregion

        #region SortedSet
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        bool SortedSetAdd<T>(string key, T value, double score);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        bool SortedSetRemove<T>(string key, T value);
        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SortedSetRangeByRank<T>(string key);

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long SortedSetLength(string key);
        #endregion

        #region key

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        //bool KeyDelete(string key);
        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        long KeyDelete(List<string> keys);
        /// <summary>
        /// 删除以<paramref name="pattern"/>开头的key
        /// </summary>
        /// <param name="pattern"></param>
        //void KeyDeleteStartWith(string pattern);
        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        //bool KeyExists(string key);
        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        bool KeyRename(string key, string newKey);
        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?));
        /// <summary>
        /// 设置过期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        bool KeyExpire(string key, DateTime dateTime);
        /// <summary>
        /// 清空缓存
        /// </summary>
        //void FlushDb();
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">锁名称，不可重复</param>
        /// <param name="value">用于释放锁的标记</param>
        /// <param name="cacheSeconds">锁有效期,秒</param>
        /// <returns></returns>
        bool LockTake(string key, string value = "", int cacheSeconds = 10);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheSeconds"></param>
        /// <returns></returns>
        bool LockTake(string key, int cacheSeconds);
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁名称</param>
        /// <param name="value">标记，必须与锁的时候一直</param>
        /// <returns></returns>
        bool LockRelease(string key, string value = "");
        /// <summary>
        /// 根据值获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        //List<string> GetKeys<T>(T value);
        #endregion

    }
}

