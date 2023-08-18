using CSharp.Net.Util;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 字典扩展
/// </summary>
public static class DictionaryExtension
{
    /// <summary>
    /// 从Dictionary中读取数据
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <returns>default null</returns>
    public static TValue GetValue<TValue>(this IDictionary<string, TValue> sd, string key)
    {
        if (sd == null || !sd.ContainsKey(key)) return default(TValue);
        //object retValue;
        //if (sd.TryGetValue(key, out retValue))
        //    return retValue;
        return sd[key];
    }

    /// <summary>
    /// 从Dictionary中读取数据
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns> default value</returns>
    public static TValue GetValue<TValue>(this IDictionary<string, TValue> sd, string key, TValue defaultValue = default(TValue))
    {
        if (sd == null || !sd.ContainsKey(key)) return defaultValue;
        return sd[key]; 
    }

    /// <summary>
    /// 从Dictionary中读取数据
    /// </summary>
    /// <typeparam name="TSource">source type</typeparam>
    /// <typeparam name="TValue">target type</typeparam>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns> default value</returns>
    public static TValue GetValue<TSource,TValue>(this IDictionary<string, TSource> sd, string key, TValue defaultValue = default(TValue))
    {
        if (sd == null || !sd.ContainsKey(key)) return defaultValue;
        var retValue = ConvertHelper.ConvertTo(sd[key], defaultValue);
        return retValue;
    }

    /// <summary>
    /// 从SortedDictionary中读取数据
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <returns>default null</returns>
    public static object GetValue(this IDictionary<string, object> sd, string key)
    {
        if (sd == null) return null;
        object retValue;
        if (sd.TryGetValue(key, out retValue))
            return retValue;
        return null;
    }

    /// <summary>
    /// 从SortedDictionary中读取数据
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns>default value</returns>
    public static TValue GetValue<TValue>(this IDictionary<string, object> sd, string key, TValue defaultValue = default(TValue))
    {
        var sdValue = sd.GetValue(key);
        var retValue = ConvertHelper.ConvertTo(sdValue, defaultValue);
        return retValue;
    }

    /// <summary>
    /// Dictionary排序
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns>ArgumentNullException</returns>
    public static SortedDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException("dictionary");
        return new SortedDictionary<TKey, TValue>(dictionary);
    }
}
