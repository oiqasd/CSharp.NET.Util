using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Cache
{
    public interface ICache
    {

        /// <summary>
        /// 根据Key索引值，获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key] { get; set; }

        /// <summary>
        /// 获取缓存总数据项的个数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 保存单个key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="isSliding"></param>
        bool StringSet(string key, string value, int cacheSeconds = 0, bool isSliding = true);

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="isSliding"></param>
        /// <returns></returns>
        bool StringSet<T>(string key, T obj, int cacheSeconds = 0, bool isSliding = true) where T : new();

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T StringGet<T>(string key);

        /// <summary>
        /// 获取key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string StringGet(string key);

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        bool KeyDelete(string key);
        /// <summary>
        /// 获取或创建一个key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue">不存在则使用该值创建</param>
        /// <param name="seconds">绝对过期时间 默认30秒</param>
        /// <returns></returns>
        string GetOrCreate(string key, string defaultValue, int seconds = 30);
        /// <summary>
        /// 删除以<paramref name="pattern"/>开头的key
        /// </summary>
        /// <param name="pattern"></param>
        void KeyDeleteStartWith(string pattern);
        /// <summary>
        /// 查询<paramref name="pattern"/>开头的keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removePrefix">默认移除实例前缀</param>
        /// <returns></returns>
        string[] QueryStartWith(string pattern, bool removePrefix = true);
        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        bool KeyExists(string key);

        /// <summary>
        /// 清空缓存
        /// </summary>
        void FlushDb();

        /// <summary>
        /// 根据值获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        List<string> GetKeys<T>(T value);

        #region Set 无序集合

        #region 同步方法

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
        /// <summary>
        /// 并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetUnion<T>(params string[] key);
        /// <summary>
        /// 交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetIntersect<T>(params string[] key);
        /// <summary>
        /// 差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetDifference<T>(params string[] key);
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
        Task<bool> SetAddAsync<T>(string key, T[] value, TimeSpan? timeSpan = null);
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

        #endregion 异步方法

        #endregion Set 无序集合


        #region 发布订阅

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        void Subscribe(string subChannel);

        /// <summary>
        /// 发布订阅 
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="action"></param>
        void Subscribe(string subChannel, Action<string> action);
        Task SubscribeAsync(string subChannel, Func<string, Task> action);

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        long Publish<T>(string channel, T msg);

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        void Unsubscribe(string channel);

        /// <summary>
        /// [慎重调用]Redis发布订阅  取消全部订阅
        /// </summary>
        void UnsubscribeAll();
        #endregion
    }
}

