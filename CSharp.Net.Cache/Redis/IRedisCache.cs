using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharp.Net.Cache
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
        //T StringGetAsync<T>(string key);
        /// <summary>
        /// 获取key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //string StringGetAsync(string key);
        /// <summary>
        /// 批量读
        /// </summary>
        Task<Dictionary<string, string>> StringGetAsync(string[] keys); 

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

        Task<T> StringGetAsync<T>(string key);
        Task<string> StringGetAsync(string key);
        Task<bool> StringSetAsync(string key, string value, int cacheSeconds = 0);
        Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry);
        Task<bool> StringSetAsync<T>(string key, T obj, TimeSpan? expiry) where T : new();
        Task<double> StringIncrementAsync(string key, double val = 1, TimeSpan? expireTimeSpan = null, bool updateExpire = false);
        Task<double> StringDecrementAsync(string key, double val = 1);
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
        /// Removes and returns the specified number of elements from the end of a list stored at the given key.
        /// </summary>
        /// <typeparam name="T">The type of elements stored in the list.</typeparam>
        /// <param name="key">The key identifying the list from which elements will be removed. Cannot be null or empty.</param>
        /// <param name="count">The number of elements to remove from the end of the list. Must be greater than or equal to 0.</param>
        /// <returns>A list of elements removed from the end of the list. If the key does not exist or the list is empty, returns an
        /// empty list.</returns>
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
        /// 左出
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
        /// <summary>
        /// Removes all occurrences of the specified value from the list stored at the given key.
        /// </summary>
        /// <typeparam name="T">The type of the value to remove from the list.</typeparam>
        /// <param name="key">The key identifying the list from which the value will be removed. Cannot be null or empty.</param>
        /// <param name="value">The value to remove from the list. The comparison is based on the default equality comparer for the type
        /// <typeparamref name="T"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of removed
        /// occurrences of the specified value. Returns 0 if the value was not found in the list.</returns>
        Task<long> ListRemoveAsync<T>(string key, T value);

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <remarks>The range is inclusive, meaning both the <paramref name="start"/> and <paramref name="stop"/> indices
        /// are included in the result. Negative indices can be used to specify offsets from the end of the list. For example,
        /// -1 represents the last element, -2 represents the second-to-last element, and so on.</remarks>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="key">The key identifying the list in the data store. Cannot be null or empty.</param>
        /// <param name="start">The zero-based index of the first element to retrieve. Defaults to 0.</param>
        /// <param name="stop">The zero-based index of the last element to retrieve. A value of -1 indicates the end of the list. Defaults to -1.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of elements of type <typeparamref
        /// name="T"/> within the specified range. If the key does not exist, an empty list is returned.</returns>
        Task<List<T>> ListRangeAsync<T>(string key, int start = 0, int stop = -1);
        /// <summary>
        /// Adds the specified value to the end of the list stored at the given key.
        /// </summary>
        /// <remarks>If the key does not exist, a new list will be created and the value will be added to
        /// it.</remarks>
        /// <typeparam name="T">The type of the value to be added to the list.</typeparam>
        /// <param name="key">The key identifying the list. Cannot be null or empty.</param>
        /// <param name="value">The value to add to the end of the list.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the length of the list after the
        /// push operation.</returns>
        Task<long> ListRightPushAsync<T>(string key, T value);
        /// <summary>
        /// Removes and returns the last element of the list stored at the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="key">The key identifying the list in the data store. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last element of the list, or
        /// <see langword="default"/> if the list is empty or the key does not exist.</returns>
        Task<T> ListRightPopAsync<T>(string key);
        /// <summary>
        /// Removes and returns the specified number of elements from the end of a list stored at the given key.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="key">The key identifying the list in the data store. Cannot be null or empty.</param>
        /// <param name="count">The number of elements to remove and return. Must be greater than zero.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of elements  removed from
        /// the end of the list. If the key does not exist or the list is empty, an empty list is returned.</returns>
        Task<List<T>> ListRightPopAsync<T>(string key, int count);
        /// <summary>
        /// Inserts the specified value at the head of the list stored at the given key.
        /// </summary>
        /// <remarks>If the key does not exist, a new list will be created and the value will be added as
        /// its first element.</remarks>
        /// <typeparam name="T">The type of the value to be inserted into the list.</typeparam>
        /// <param name="key">The key identifying the list. Must not be null or empty.</param>
        /// <param name="value">The value to insert at the head of the list. Must not be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the length of the list after the
        /// push operation.</returns>
        Task<long> ListLeftPushAsync<T>(string key, T value);
        /// <summary>
        /// Removes and returns the first element from the list stored at the specified key.
        /// </summary>
        /// <remarks>This method interacts with a data store to retrieve and remove the first element of
        /// the list. The key is automatically prefixed before accessing the data store.</remarks>
        /// <typeparam name="T">The type of the element to be returned.</typeparam>
        /// <param name="key">The key identifying the list. The key cannot be null or empty.</param>
        /// <returns>The first element of the list, deserialized to the specified type <typeparamref name="T"/>. Returns the
        /// default value of <typeparamref name="T"/> if the list is empty or the key does not exist.</returns>
        Task<T> ListLeftPopAsync<T>(string key);
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
        Task<List<T>> ListLeftPopAsync<T>(string key, int count);
        /// <summary>
        /// Asynchronously retrieves the length of the list stored at the specified key.
        /// </summary>
        /// <param name="key">The key identifying the list in the data store. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of elements in the
        /// list. Returns 0 if the list does not exist.</returns>
        Task<long> ListLengthAsync(string key);
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
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        Task<bool> HashExistsAsync(string key, string dataKey);
        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <param name="expired"></param>
        /// <returns></returns>
        Task<bool> HashSetAsync<T>(string key, string dataKey, T t, TimeSpan? expired = null);
        /// <summary>
        /// 移除hash中值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        Task<bool> HashDeleteAsync(string key, string dataKey);
        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        Task<T> HashGetAsync<T>(string key, string dataKey);
        /// <summary>
        /// 异步自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <param name="expired"></param>
        /// <returns></returns>
        Task<double> HashIncrementAsync(string key, string dataKey, double val = 1, TimeSpan? expired = null);
        /// <summary>
        /// 异步自减
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        Task<double> HashDecrementAsync(string key, string dataKey, double val = 1);
        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<T>> HashKeysAsync<T>(string key);
        /// <summary>
        /// 获取hashkey所有Redis key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);
        #endregion

        #region SortedSet
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        Task<bool> SortedSetAdd<T>(string key, T value, double score);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        Task<bool> SortedSetRemove<T>(string key, T value);
        /// <summary>
        /// Get SortedSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="sortAsc"></param>
        /// <returns></returns>
        Task<List<T>> SortedSetRangeByRank<T>(string key, long start = 0, long stop = -1, bool sortAsc = true);

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> SortedSetLength(string key);
        #endregion

        #region key
        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        Task<long> KeyDeleteAsync(List<string> keys);
        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> KeyDeleteAsync(string key);
        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> KeyExistsAsync(string key);
        /*
          /// <summary>
          /// 删除单个key
          /// </summary>
          /// <param name="key">redis key</param>
          /// <returns>是否删除成功</returns>
          //bool KeyDelete(string key);
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
          /// 清空缓存
          /// </summary>
          //void FlushDb();
          */
        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        Task<bool> KeyRenameAsync(string key, string newKey);
        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        bool KeyExpire(string key, TimeSpan? expiry = null);
        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        Task<bool> KeyExpireAsync(string key, TimeSpan? expiry = null);
        /// <summary>
        /// 获取Key的过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<DateTime?> KeyExpireTimeAsync(string key);
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">锁名称，不可重复</param>
        /// <param name="value">用于释放锁的标记</param>
        /// <param name="cacheSeconds">锁有效期,秒</param>
        /// <returns></returns>
        bool LockTake(string key, int cacheSeconds = 10, string value = "");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> LockTakeAsync(string key, int cacheSeconds = 10, string value = "");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="waitSeconds"></param>
        /// <returns></returns>
        void LockWait(string key, int waitSeconds = 10);
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁名称</param>
        /// <param name="value">标记，必须与锁的时候一直</param>
        /// <returns></returns>
        bool LockRelease(string key, string value = "");
        Task<bool> LockReleaseAsync(string key, string value = "");
        /*
        /// <summary>
        /// 根据值获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        List<string> GetKeys<T>(T value);
        */
        #endregion

        #region Set 无序集合

        #region 同步方法
        /*
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        bool SetAdd<T>(string key, T value, TimeSpan? timeSpan = null);

        /// <summary>
        /// 向set添加数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan = null);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        long SetRemove<T>(string key, params T[] value);

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetMembers<T>(string key);

        /// <summary>
        /// 随机获取一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T SetRandomMember<T>(string key);

        /// <summary>
        /// 随机获取多个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SetRandomMembers<T>(string key, long count = 1);

        /// <summary>
        /// 判断key集合中是否包含指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetContains<T>(string key, T value);
        */

        /// <summary>
        ///  随机删除key集合中的一个值，并返回该值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param> 
        /// <returns></returns>
        T SetPop<T>(string key);

        /// <summary>
        /// 随机删除key集合中的count个值，并返回count个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SetPop<T>(string key, long count);

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        long SetLength(string key);

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        Task<bool> SetAddAsync<T>(string key, T value, TimeSpan? timeSpan = null);
        /// <summary>
        /// 向set添加数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        Task<long> SetAddAsync<T>(string key, T[] value, TimeSpan? timeSpan = null);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        Task<long> SetRemoveAsync<T>(string key, params T[] value);

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<T>> SetMembersAsync<T>(string key);

        /// <summary>
        /// 随机获取一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> SetRandomMemberAsync<T>(string key);

        /// <summary>
        /// 随机获取多个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<T>> SetRandomMembersAsync<T>(string key, long count = 1);

        /// <summary>
        /// 判断key集合中是否包含指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> SetContainsAsync<T>(string key, T value);

        /// <summary>
        ///  随机删除key集合中的一个值，并返回该值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param> 
        /// <returns></returns>
        Task<T> SetPopAsync<T>(string key);

        /// <summary>
        /// 随机删除key集合中的count个值，并返回count个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<T>> SetPopAsync<T>(string key, long count);

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<long> SetLengthAsync(string key);

        /// <summary>
        /// 并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<T>> SetUnionAsync<T>(params string[] key);
        /// <summary>
        /// 交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<T>> SetIntersectAsync<T>(params string[] key);
        /// <summary>
        /// 差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<List<T>> SetDifferenceAsync<T>(params string[] key);
        #endregion 异步方法

        #endregion Set 无序集合

        #region 发布订阅

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        void Subscribe(string subChannel);
        /// <summary>
        /// 消息订阅 
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        void Subscribe(string subChannel, Action<string> handler);
        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        void Subscribe<T>(string subChannel, Action<T> handler);
        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        Task SubscribeAsync(string subChannel, Func<string, Task> handler);
        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(string subChannel, Func<T, Task> handler);
        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(string subChannel, Action<T> handler);

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        long Publish<T>(string channel, T msg);
        /// <summary>
        /// 发布订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task<long> PublishAsync<T>(string channel, T msg);
        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        Task Unsubscribe(string channel);

        /// <summary>
        /// [慎重调用]Redis发布订阅  取消全部订阅
        /// </summary>
        Task UnsubscribeAll();
        #endregion
    }
}

