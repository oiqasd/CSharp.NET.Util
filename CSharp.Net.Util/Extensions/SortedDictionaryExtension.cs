using CSharp.Net.Util;
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
    public static object GetValue(this Dictionary<string, object> sd, string key)
    {
        if (sd == null) return null;
        object retValue;
        if (sd.TryGetValue(key, out retValue))
            return retValue;
        return null;
    }

    /// <summary>
    /// 从Dictionary中读取数据
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns> default value</returns>
    public static TValue GetValue<TValue>(this Dictionary<string, object> sd, string key, TValue defaultValue = default(TValue))
    {
        var sdValue = sd.GetValue(key);
        var retValue = ConvertHelper.ConvertTo(sdValue, defaultValue);

        return retValue;
    }

    /// <summary>
    /// 从SortedDictionary中读取数据
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <returns>default null</returns>
    public static object GetValue(this SortedDictionary<string, object> sd, string key)
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
    public static TValue GetValue<TValue>(this SortedDictionary<string, object> sd, string key, TValue defaultValue = default(TValue))
    {
        var sdValue = sd.GetValue(key);
        var retValue = ConvertHelper.ConvertTo(sdValue, defaultValue);
        return retValue;
    }
}
