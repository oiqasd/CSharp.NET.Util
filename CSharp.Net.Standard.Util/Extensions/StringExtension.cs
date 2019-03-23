using CSharp.Net.Standard.Util;
using System;

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
    /// </param>
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
    /// </param>
    /// <returns></returns>
    public static string ToString(this DateTime? time, int formatType, string defaultvalue = "")
    {
        if (time == null)
            return defaultvalue;
        return ((DateTime)time).ToString(formatType, defaultvalue);
    }

    public static string ToString(this string str, string format)
    {
        if (str == null)
            return "";
        return str;
    }
     
}
