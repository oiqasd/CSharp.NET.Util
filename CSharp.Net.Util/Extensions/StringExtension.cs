using CSharp.Net.Util;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

/// <summary>
/// StringExtension
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// 时间格式转换
    /// <param name="time"></param>
    /// <param name="formatType">
    /// 1:yyyy-MM-dd HH:mm:ss 
    /// 2:yyyy-MM-dd 
    /// 3:yyyy-MM-dd HH:mm:ss.ffff
    /// 4:yyyyMMddHHmmss
    /// 5:yyyyMMddHHmmssffff
    /// 6.yyyy年MM月dd日
    /// 7.yyyy年MM月dd日HH时mm分ss秒
    /// 8.yyyyMMdd
    /// 9.yyyy-MM-dd HH:mm
    /// 10.HH:mm:ss
    /// 11.HH:mm
    /// other return 1.
    /// </param>
    /// <param name="defaultvalue"></param>
    /// <returns></returns>
    /// </summary>
    public static string ToString(this DateTime time, int formatType, string defaultvalue = "")
    {
        string dateStr = defaultvalue;
        if (time == default(DateTime))
            return dateStr;
        if (time == DateTimeHelper.BaseDateTime)
            return dateStr;
        switch (formatType)
        {
            case 1:
                dateStr = time.ToString("yyyy-MM-dd HH:mm:ss");
                break;
            case 2:
                dateStr = time.ToString("yyyy-MM-dd");
                break;
            case 3:
                dateStr = time.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                break;
            case 4:
                dateStr = time.ToString("yyyyMMddHHmmss");
                break;
            case 5:
                dateStr = time.ToString("yyyyMMddHHmmssffff");
                break;
            case 6:
                dateStr = time.ToString("yyyy年MM月dd日");
                break;
            case 7:
                dateStr = time.ToString("yyyy年MM月dd日HH时mm分ss秒");
                break;
            case 8:
                dateStr = time.ToString("yyyyMMdd");
                break;
            case 9:
                dateStr = time.ToString("yyyy-MM-dd HH:mm");
                break;
            case 10:
                dateStr = time.ToString("HH:mm:ss");
                break;
            case 11:
                dateStr = time.ToString("HH:mm");
                break;
            default:
                dateStr = time.ToString("yyyy-MM-dd HH:mm:ss");
                break;
        }
        return dateStr;
    }

    /// <summary>
    /// 时间格式转换
    /// <param name="time"></param>
    /// <param name="formatType">
    /// 1:yyyy-MM-dd HH:mm:ss 
    /// 2:yyyy-MM-dd 
    /// 3:yyyy-MM-dd HH:mm:ss.ffff
    /// 4:yyyyMMddHHmmss
    /// 5:yyyyMMddHHmmssffff
    /// 6.yyyy年MM月dd日
    /// 7.yyyy年MM月dd日HH时mm分ss秒
    /// 8.yyyyMMdd
    /// 9.yyyy-MM-dd HH:mm
    /// </param>
    /// <param name="defaultvalue"></param>
    /// <returns></returns>
    /// </summary>
    public static string ToString(this DateTime? time, int formatType, string defaultvalue = "")
    {
        if (time == null)
            return defaultvalue;
        return ((DateTime)time).ToString(formatType, defaultvalue);
    }

    /// <summary>
    /// 字符串转换
    /// <param name="obj"></param>
    /// <returns>如果obj是null,返回string.Empty </returns>
    /// </summary>
    public static string ToString(this object obj)
    {
        if (obj == null)
            return string.Empty;
        return obj.ToString();
    }

    /// <summary>
    /// 隐藏字符串
    /// <param name="str"></param>
    /// <param name="showStart">显示头几位,默认全部,不够则全显</param>
    /// <param name="showEnd">显示尾几位,默认全部</param>
    /// <returns></returns>
    /// </summary>
    public static string HideStr(this string str, uint showStart = 0, uint showEnd = 0)
    {
        if (string.IsNullOrWhiteSpace(str)) return "";

        if (str.Length <= showStart) return str;

        StringBuilder newstr = new StringBuilder();
        if (showStart > 0)
            newstr.Append(str.Substring(0, (int)showStart));

        for (int i = 0; i < (str.Length - showStart - showEnd); i++)
        {
            newstr.Append("*");
        }

        if (showEnd > 0)
            newstr.Append(str.Substring(str.Length - (int)showEnd));

        return newstr.ToString();
    }

    /// <summary>
    /// 扩展，转换为INT32
    /// <param name="s"></param>
    /// <returns></returns>
    /// </summary>
    public static int ToInt(this string s)
    {
        if (s.Contains("."))
            return Convert.ToInt32(Math.Round(decimal.Parse(s), 0));
        return int.Parse(s);
    }

    /// <summary>
    /// 转int
    /// <param name="s"></param>
    /// <returns></returns>
    /// </summary>
    public static int TryInt(this string s)
    {
        int.TryParse(s, out int result);
        return result;
    }
    /// <summary>
    /// 转decimal
    /// <param name="s"></param>
    /// <returns></returns>
    /// </summary>
    public static decimal TryDecimal(string s)
    {
        decimal.TryParse(s, out decimal result);
        return result;
    }

    /// <summary>
    /// 字符串转为长整型
    /// <param name="s"></param>
    /// <returns>非:0</returns>
    /// </summary>
    public static long ToLong(this string s)
    {
        if (s.Contains("."))
            return Convert.ToInt64(Math.Round(decimal.Parse(s), 0));
        long.TryParse(s, out long ret);
        return ret;
    }

    /// <summary>
    /// 扩展，转换为Decimal
    /// <param name="s"></param>
    /// <returns></returns>
    /// </summary>
    public static decimal ToDecimal(this string s)
    {
        if (s.IsNullOrEmpty())
            return 0;
        return decimal.Parse(s);
    }
    /// <summary>
    /// 扩展，转换为DateTime
    /// <param name="val"></param>
    /// <returns></returns>
    /// </summary>
    public static DateTime ToDateTime(this string val)
    {
        return DateTime.Parse(val);
    }

    /// <summary>
    /// 时间转换
    /// <param name="val">yyyyMMdd、yyyy-MM-dd、yyyyMM、yyyyMMddHHmmss、
    /// yyyy-MM-dd HH:mm:ss、yyyyMMdd HH:mm:ss、yyyyMMddHHmmssffff</param>
    /// <returns><para>return DateTime</para></returns>
    /// </summary>
    public static DateTime? ConvertToDateTime(this string val)
    {
        if (val.IsNotNullOrEmpty())
        {
            return null;
        }
        bool ck = DateTime.TryParse(val, out DateTime dt);
        if (!ck)
        {
            string[] format = { "yyyyMMdd", "yyyy-MM-dd", "yyyyMM", "yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss", "yyyyMMdd HH:mm:ss", "yyyyMMddHHmmssffff" };
            //IFormatProvider ifp = new CultureInfo("zh-CN", true);
            //ck = DateTime.TryParseExact(val, format, ifp, DateTimeStyles.None, out dt);
            ck = DateTime.TryParseExact(val, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

            if (!ck)
            {
                return null;
            }
        }
        return dt;

    }

    /// <summary>
    /// 扩展，是否为空
    /// <param name="s"></param>
    /// <returns></returns>
    /// </summary>
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// 扩展，是否不为空
    /// <param name="s"></param>
    /// <returns></returns>
    /// </summary>
    public static bool IsNotNullOrEmpty(this string s)
    {
        return string.IsNullOrWhiteSpace(s) == false;
    }

    /// <summary>
    /// 格式化字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string Format(this string str, params object[] args)
    {
        return args == null || args.Length == 0 ? str : string.Format(str, args);
    }

#if NET
    /// <summary>
    /// 首字母小写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToLowerCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return string.Concat(str.First().ToString().ToLower(), str.AsSpan(1));
    }

    /// <summary>
    /// 首字母大写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToUpperCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return string.Concat(str.First().ToString().ToUpper(), str.AsSpan(1));
    }
#endif

    /// <summary>
    /// 清除字符串前后缀
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="pos">0：前后缀，1：后缀，-1：前缀</param>
    /// <param name="affixes">前后缀集合</param>
    /// <returns></returns>
    public static string ClearStringAffixes(this string str, int pos = 0, params string[] affixes)
    {
        // 空字符串直接返回
        if (string.IsNullOrWhiteSpace(str)) return str;

        // 空前后缀集合直接返回
        if (affixes == null || affixes.Length == 0) return str;

        var startCleared = false;
        var endCleared = false;

        string tempStr = null;
        foreach (var affix in affixes)
        {
            if (string.IsNullOrWhiteSpace(affix)) continue;

            if (pos != 1 && !startCleared && str.StartsWith(affix, StringComparison.OrdinalIgnoreCase))
            {
                tempStr = str[affix.Length..];
                startCleared = true;
            }
            if (pos != -1 && !endCleared && str.EndsWith(affix, StringComparison.OrdinalIgnoreCase))
            {
                var _tempStr = !string.IsNullOrWhiteSpace(tempStr) ? tempStr : str;
                tempStr = _tempStr[..^affix.Length];
                endCleared = true;
            }
            if (startCleared && endCleared) break;
        }
        return !string.IsNullOrWhiteSpace(tempStr) ? tempStr : str;
    }

    /// <summary>
    /// 反转字符串
    /// </summary>
    /// <param name="value"></param>
    public static string ToReverse(this string value) =>
          StringHelper.Reverse(value);

    /// <summary>
    /// Converts the string to a byte-array using the supplied encoding
    /// </summary>
    /// <param name = "value">The input string.</param>
    /// <param name = "encoding">The encoding to be used.Default: Encoding.Default</param>
    /// <returns>The created byte array</returns>
    /// <example>
    /// 	<code>
    /// 		var value = "Hello World";
    /// 		var ansiBytes = value.ToBytes(Encoding.GetEncoding(1252)); // 1252 = ANSI
    /// 		var utf8Bytes = value.ToBytes(Encoding.UTF8);
    /// 	</code>
    /// </example>
    public static byte[] ToBytes(this string value, string encoding = null) =>
          StringHelper.GetBytes(value, encoding);

    /// <summary>
    ///获取json对象下属性值
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="obj">json对象</param>
    /// <param name="key">字段名</param>
    /// <returns></returns>  
    public static T GetProperty<T>(this string obj, string key)
       => ConvertHelper.ConvertTo<T>(JsonHelper.GetFieldValue(obj, key));
}
