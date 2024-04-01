using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Array 扩展方法
/// </summary>
public static class ArrayExtension
{
    /// <summary>
    /// 根据id生成 in 字符串
    /// 直接拼接Sql语句中（参数化需使用find_in_set函数，由于find_in_set函数效率低，不建议使用）
    /// </summary>
    /// <param name="ids">id集合</param>
    /// <param name="defaultValue">默认值（当ids值无效时，使用默认值）</param>
    /// <returns></returns>
    public static string IdsToInSql(this IList<long> ids, IList<long> defaultValue)
    {
        var result = @"''";
        if (ids.IsHasValue())
        {
            result = string.Join(",", ids);
        }
        else
        {
            result = string.Join(",", defaultValue);
        }

        return result;
    }

    /// <summary>
    /// 根据id生成 in 字符串
    /// 直接拼接Sql语句中（参数化需使用find_in_set函数，由于find_in_set函数效率低，不建议使用）
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public static string IdsToInSql(this IList<long> ids)
    {
        var result = @"''";
        if (ids.IsHasValue())
        {
            result = string.Join(",", ids);
        }
        else
        {
            result = "-1";
        }

        return result;
    }

    /// <summary>
    /// 根据id生成 in 字符串
    /// 直接拼接Sql语句中（参数化需使用find_in_set函数，由于find_in_set函数效率低，不建议使用）
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public static string IdsToInSql(this IList<int> ids)
    {
        var result = @"''";
        if (ids.IsHasValue())
        {
            result = string.Join(",", ids);
        }
        else
        {
            result = "0";
        }

        return result;
    }

    /// <summary>
    /// 去除枚举数组中的非法项
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IList<T> GetValidateEnumList<T>(this IList<T> list)
    {
        var result = new List<T>();
        foreach (var item in list)
        {
            if (Enum.IsDefined(typeof(T), Convert.ToInt32(item)))
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>
    /// 将数组转换成以,分隔的字符串（如1,2,3）
    /// </summary>
    /// <param name="list">list</param>
    /// <returns>字符串</returns>
    public static string ToStringValue(this IList<string> list)
    {
        if (list.IsHasValue())
        {
            return string.Join(",", list);
        }
        return "";
    }

    /// <summary>
    /// 将数组转换成以自定义字符串分隔的字符串（如1/2/3）
    /// </summary>
    /// <param name="list">list</param>
    /// <param name="value">选定字符串</param>
    /// <returns>字符串</returns>
    public static string ToStringValue(this IList<string> list, string value)
    {
        if (list.IsHasValue())
        {
            return string.Join(value, list);
        }
        return "";
    }

    /// <summary>
    /// 字节数组转字符串
    /// </summary>
    /// <param name="byteList">byteList</param>
    /// <returns></returns>
    public static string ToStringValue(this IList<byte> byteList)
    {
        List<string> list = new List<string>();
        foreach (byte b in byteList)
        {
            list.Add(b.ToString("X2"));
        }
        return string.Join(" ", list);
    }

    /// <summary>
    /// 字符串分割,支持多种分割符号
    /// </summary>
    /// <param name="value"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string[] Split(this string value, string separator)
    {
        return value.Split(separator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// HashSet去重
    /// list.Distinct(a => new { a.x, a.y })
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var hashSet = new HashSet<TKey>();

        foreach (TSource element in source)
        {
            if (hashSet.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    #region 数组复制
    /// <summary>
    /// 数组复制
    /// </summary>
    public static void CopyTo(this byte[] bArrSource, List<byte> listTarget, int sourceIndex, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (sourceIndex + i < bArrSource.Length)
            {
                listTarget.Add(bArrSource[sourceIndex + i]);
            }
        }
    }

    public static void CopyTo(this List<byte> listSource, byte[] bArrTarget, int sourceIndex, int targetIndex, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (targetIndex + i < bArrTarget.Length && sourceIndex + i < listSource.Count)
            {
                bArrTarget[targetIndex + i] = listSource[sourceIndex + i];
            }
        }
    }

    public static void CopyTo(this byte[] bArrSource, byte[] bArrTarget, int sourceIndex, int targetIndex, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (targetIndex + i < bArrTarget.Length && sourceIndex + i < bArrSource.Length)
            {
                bArrTarget[targetIndex + i] = bArrSource[sourceIndex + i];
            }
        }
    }
    #endregion

    /// <summary>
    /// 数组追加
    /// </summary>
    public static List<byte> Append(this List<byte> list, byte[] bArr)
    {
        for (int i = 0; i < bArr.Length; i++)
        {
            list.Add(bArr[i]);
        }
        return list;
    }


    /// <summary>
    /// [推荐]使用IEqualityComparer通过指定属性去重
    /// list.Distinct((a, b) => a.x == b.x &amp;&amp; a.y == b.y)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparer) where T : class
       => source.Distinct(new DynamicEqualityComparer<T>(comparer));

    private sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        private readonly Func<T, T, bool> _func;

        public DynamicEqualityComparer(Func<T, T, bool> func)
        {
            _func = func;
        }

        public bool Equals(T x, T y) => _func(x, y);

        public int GetHashCode(T obj) => 0;
    }
}

