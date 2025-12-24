using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;

public static class DateTimeExtension
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
    /// 12.HHmmss
    /// 13.yyyy-MM
    /// 14.yyyyMM
    /// 15.yyyy/MM/dd HH:mm:ss
    /// 16.yyyy/MM/dd
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
        return formatType switch
        {
            1 => time.ToString("yyyy-MM-dd HH:mm:ss"),
            2 => time.ToString("yyyy-MM-dd"),
            3 => time.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
            4 => time.ToString("yyyyMMddHHmmss"),
            5 => time.ToString("yyyyMMddHHmmssffff"),
            6 => time.ToString("yyyy年MM月dd日"),
            7 => time.ToString("yyyy年MM月dd日HH时mm分ss秒"),
            8 => time.ToString("yyyyMMdd"),
            9 => time.ToString("yyyy-MM-dd HH:mm"),
            10 => time.ToString("HH:mm:ss"),
            11 => time.ToString("HH:mm"),
            12 => time.ToString("HHmmss"),
            13 => time.ToString("yyyy-MM"),
            14 => time.ToString("yyyyMM"),
            15 => time.ToString("yyyy/MM/dd HH:mm:ss"),
            16 => time.ToString("yyyy/MM/dd"),
            _ => time.ToString("yyyy-MM-dd HH:mm:ss"),
        };
        /*
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
            case 12:
                dateStr = time.ToString("HHmmss");
                break;
            default:
                dateStr = time.ToString("yyyy-MM-dd HH:mm:ss");
                break;
        }
        return dateStr;
        */
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
    /// 日期部分转化为数字
    /// </summary>
    /// <returns>yyyyMMdd</returns>
    public static int ToyyyyMMdd(this DateTime date)
    {
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }

    /// <summary>
    /// 月部分转化为数字
    /// </summary>
    /// <returns>yyyyMM</returns>
    public static int ToyyyyMM(this DateTime date)
    {
        return date.Year * 100 + date.Month;
    }

    /// <summary>
    /// 时间部分转化为数字
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>HHmmss</returns>
    public static int ToHHmmss(this DateTime dateTime)
    {
        return dateTime.ToString(12).ToInt();
    }
}

