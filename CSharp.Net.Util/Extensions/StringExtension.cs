using CSharp.Net.Util;
using System;
using System.Globalization;
using System.Text;

public static class StringExtension
{
    /// <summary>
    /// 时间格式转换
    /// </summary>
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
    /// </param>
    /// <param name="defaultvalue"></param>
    /// <returns></returns>
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
    /// </summary>
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
    /// <returns></returns>
    public static string ToString(this DateTime? time, int formatType, string defaultvalue = "")
    {
        if (time == null)
            return defaultvalue;
        return ((DateTime)time).ToString(formatType, defaultvalue);
    }

    /// <summary>
    /// 字符串转换
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>如果obj是null,返回string.Empty </returns>
    public static string ToString(this object obj)
    {
        if (obj == null)
            return string.Empty;
        return obj.ToString();
    }


    /// <summary>
    /// 隐藏字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="showStart">显示头几位,默认全部,不够则全显</param>
    /// <param name="showEnd">显示尾几位,默认全部</param>
    /// <returns></returns>
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
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int ToInt(this string s)
    {
        if (s.Contains("."))
            return Convert.ToInt32(Math.Round(decimal.Parse(s), 0));
        return int.Parse(s);
    }

    /// <summary>
    /// 转int
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int TryInt(this string s)
    {
        int.TryParse(s, out int result);
        return result;
    }
    /// <summary>
    /// 转decimal
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static decimal TryDecimal(string s)
    {
        decimal.TryParse(s, out decimal result);
        return result;
    }

    /// <summary>
    /// 字符串转为长整型
    /// </summary>
    /// <param name="s"></param>
    /// <returns>非:0</returns>
    public static long ToLong(this string s)
    {
        if (s.Contains("."))
            return Convert.ToInt64(Math.Round(decimal.Parse(s), 0));
        long.TryParse(s, out long ret);
        return ret;
    }

    /// <summary>
    /// 扩展，转换为Decimal
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static decimal ToDecimal(this string s)
    {
        if (s.IsNullOrEmpty())
            return 0;
        return decimal.Parse(s);
    }
    /// <summary>
    /// 扩展，转换为DateTime
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(this string val)
    {
        return DateTime.Parse(val);
    }

    /// <summary>
    /// 时间转换
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
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
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// 扩展，是否不为空
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsNotNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s) == false;
    }
}
