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
    /// <param name="showStart">显示头几位,默认全部隐藏,不够则全显</param>
    /// <param name="showEnd">显示尾几位,默认全部隐藏</param>
    /// <param name="replaceChar">显示字符</param>
    /// <returns></returns>
    /// </summary>
    public static string HideStr(this string str, uint showStart = 0, uint showEnd = 0, char replaceChar = '*')
    {
        if (string.IsNullOrWhiteSpace(str)) return replaceChar.ToString();
        if (str.Length <= showStart) return str;
#if NET
        string nstr = string.Create(str.Length, str.ToCharArray(), (Span<char> strContent, char[] charArray) =>
        {
            uint len = (uint)charArray.Length;
            for (int i = 0; i < len; i++)
            {
                if (i >= showStart && (i < len - showEnd || showEnd == 0))
                    strContent[i] = replaceChar;
                else
                    strContent[i] = charArray[i];
            }
        });
        return nstr;
#else
        StringBuilder newstr = new StringBuilder();
        if (showStart > 0)
            newstr.Append(str.Substring(0, (int)showStart));
        for (int i = 0; i < (str.Length - showStart - showEnd); i++)
        {
            newstr.Append(replaceChar);
        }
        if (showEnd > 0)
            newstr.Append(str.Substring(str.Length - (int)showEnd));
        return newstr.ToString();
#endif
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
#endif

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

    /// <summary>
    /// 右边加长度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length"></param>
    /// <param name="paddingChar"></param>
    /// <returns></returns>
    public static string PadRFixed(this string value, int length = 0, char paddingChar = ' ')
    {
        if (length <= 0) return value;
        if (value == null)
            value = "";
        int currentWidth = 0;
        foreach (char c in value)
        {
            currentWidth += (c > 0xFF) ? 2 : 1; // 全角为2，半角为1
        }
        int paddingLength = length - currentWidth;
        if (paddingLength <= 0) return value;
        return value + new string(paddingChar, paddingLength);
        //value.PadRight(length);
    }

    /// <summary>
    /// 左边加长度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length"></param>
    /// <param name="paddingChar"></param>
    /// <returns></returns>
    public static string PadLFixed(this string value, int length = 0, char paddingChar = ' ')
    {
        if (length <= 0) return value;
        if (value == null)
            value = "";
        int currentWidth = 0;
        foreach (char c in value)
        {
            currentWidth += (c > 0xFF) ? 2 : 1; // 全角为2，半角为1
        }
        int paddingLength = length - currentWidth;
        if (paddingLength <= 0) return value;
        return new string(paddingChar, paddingLength) + value;
        //value.PadRight(length);
    }
}
