using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;

public static class SortedDictionaryExtension
{
    /// <summary>
    /// 从Dictionary中读取数据
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object GetValue(this SortedDictionary<string, object> sd, string key)
    {
        object retValue;
        if (sd.TryGetValue(key, out retValue))
            return retValue;
        return null;
    }

    public static TValue GetValue<TValue>(this SortedDictionary<string, object> sd, string key, TValue defaultValue = default(TValue))
    {
        var sdValue = sd.GetValue(key);

        var retValue = ConvertHelper.ConvertTo<TValue>(sdValue, defaultValue);

        return retValue;
    }
}
