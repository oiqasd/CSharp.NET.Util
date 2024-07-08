using System;
using System.Collections.Generic;
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

        ///// <summary>
        ///// 保存一个对象
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="obj"></param>
        ///// <param name="cacheSeconds"></param>
        ///// <param name="isSliding"></param>
        ///// <returns></returns>
        //bool StringSet<T>(string key, T obj, int cacheSeconds = 0, bool isSliding = true) where T : new();

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="cacheSeconds">默认0，不过期</param>
        /// <returns></returns>
        bool StringSet(string key, object obj, int cacheSeconds = 0);
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
        /// 获取或添加key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null);// where T : new();
        /// <summary>
        /// 获取或创建一个key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="func">不存在则使用该值创建</param>
        /// <param name="seconds">绝对过期时间 默认0秒不过期</param>
        /// <returns></returns>
        T GetOrSet<T>(string key, Func<T> func, int seconds = 0);
        /// <summary>
        /// 删除以<paramref name="pattern"/>开头的key
        /// </summary>
        /// <param name="pattern"></param>
        Task KeyDeleteStartWith(string pattern);
        /// <summary>
        /// 查询<paramref name="pattern"/>开头的keys
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removePrefix">默认移除实例前缀</param>
        /// <returns></returns>
        Task<string[]> QueryStartWith(string pattern, bool removePrefix = true);
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

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        bool SetAdd<T>(string key, T value, TimeSpan? timeSpan = null);
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        bool SetAdd<T>(string key, T[] value, TimeSpan? timeSpan = null);
        /// <summary>
        /// 移除集合对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long SetRemove<T>(string key, params T[] value);
        /// <summary>
        /// 获取集合所有对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> SetMembers<T>(string key);

        /// <summary>
        /// 随机返回集合一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T SetRandomMember<T>(string key);
        /// <summary>
        /// 随机返回集合多个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> SetRandomMembers<T>(string key, long count = 1);
        /// <summary>
        /// 是否存在对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetContains<T>(string key, T value);

    }
}

