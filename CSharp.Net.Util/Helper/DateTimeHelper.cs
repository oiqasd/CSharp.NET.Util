using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 枚举，比对时间的类型
    /// </summary>
    public enum DateInterval
    {
        /// <summary>
        /// 秒
        /// </summary>
        Second,
        /// <summary>
        /// 分
        /// </summary>
        Minute,
        /// <summary>
        /// 时
        /// </summary>
        Hour,
        /// <summary>
        /// 日
        /// </summary>
        Day,
        /// <summary>
        /// 周
        /// </summary>
        Week,
        /// <summary>
        /// 月
        /// </summary>
        Month,
        /// <summary>
        /// 季
        /// </summary>
        Quarter,
        /// <summary>
        /// 周
        /// </summary>
        Year
    }

    /// <summary>
    /// 时间帮助类
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// 返回当前标准日期格式
        /// yyyy-MM-dd
        /// </summary>
        public static string GetNowDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 返回标准时间格式
        /// HH:mm:ss
        /// </summary>
        public static string GeNowTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间格式
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string GetNowDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回相对于当前时间的相对天数时间
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string GetDateTime(int relativeday)
        {
            return DateTime.Now.AddDays(relativeday).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间格式
        /// yyyy-MM-dd HH:mm:ss:fffffff
        /// </summary>
        public static string GetNowDateTimeF()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff");
        }

        /// <summary>
        /// 返回指定日期格式
        /// </summary>
        public static string GetDate(string datetimestr, string replacestr)
        {
            if (datetimestr == null)
                return replacestr;

            if (datetimestr.Equals(""))
                return replacestr;

            try
            {
                datetimestr = Convert.ToDateTime(datetimestr).ToString("yyyy-MM-dd").Replace("1900-01-01", replacestr);
            }
            catch
            {
                return replacestr;
            }
            return datetimestr;
        }

        /// <summary>
        /// 返回标准时间 
        /// </summary>
        /// <param name="fDateTime"></param>
        /// <param name="formatStr"></param>
        /// <returns></returns>
        public static string GetStandardDateTime(string fDateTime, string formatStr)
        {
            if (fDateTime == "0000-0-0 0:00:00")
                return fDateTime;
            DateTime time = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            if (DateTime.TryParse(fDateTime, out time))
                return time.ToString(formatStr);
            else
                return "N/A";
        }

        /// <summary>
        /// 返回标准时间 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="fDateTime"></param>
        /// <returns></returns>
        public static string GetStandardDateTime(string fDateTime)
        {
            return GetStandardDateTime(fDateTime, "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间 yyyy-MM-dd
        /// </summary>
        /// <param name="fDate"></param>
        /// <returns></returns>
        public static string GetStandardDate(string fDate)
        {
            return GetStandardDateTime(fDate, "yyyy-MM-dd");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsTime(string timeval)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(timeval, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        /// <summary>
        /// 返回相差的秒数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="Sec"></param>
        /// <returns></returns>
        public static int StrDateDiffSeconds(string time, int Sec)
        {
            if (string.IsNullOrEmpty(time))
                return 1;
            DateTime dateTime = ConvertHelper.ConvertTo<DateTime>(time);
            if (dateTime.ToString("yyyy-MM-dd") == "1900-01-01")
                return 1;

            TimeSpan ts = DateTime.Now - dateTime.AddSeconds(Sec);
            if (ts.TotalSeconds > int.MaxValue)
                return int.MaxValue;

            else if (ts.TotalSeconds < int.MinValue)
                return int.MinValue;

            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// 返回相差的分钟数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static int StrDateDiffMinutes(string time, int minutes)
        {
            if (string.IsNullOrEmpty(time))
                return 1;
            DateTime dateTime = ConvertHelper.ConvertTo<DateTime>(time);
            if (dateTime.ToString("yyyy-MM-dd") == "1901-01-01")
                return 1;

            TimeSpan ts = DateTime.Now - dateTime.AddMinutes(minutes);
            if (ts.TotalMinutes > int.MaxValue)
                return int.MaxValue;
            else if (ts.TotalMinutes < int.MinValue)
                return int.MinValue;

            return (int)ts.TotalMinutes;
        }

        /// <summary>
        /// 返回相差的小时数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="hours"></param>
        /// <returns></returns>
        public static int StrDateDiffHours(string time, int hours)
        {
            if (string.IsNullOrEmpty(time))
                return 1;
            DateTime dateTime = ConvertHelper.ConvertTo<DateTime>(time);
            if (dateTime.ToString("yyyy-MM-dd") == "1901-01-01")
                return 1;

            TimeSpan ts = DateTime.Now - dateTime.AddHours(hours);
            if (ts.TotalHours > int.MaxValue)
                return int.MaxValue;
            else if (ts.TotalHours < int.MinValue)
                return int.MinValue;

            return (int)ts.TotalHours;
        }

        /// <summary>
        /// 将8位日期型整型数据转换为日期字符串数据
        /// </summary>
        /// <param name="date">整型日期</param>
        /// <param name="chnType">是否以中文年月日输出</param>
        /// <returns></returns>
        public static string FormatDate(int date, bool chnType = false)
        {
            string dateStr = date.ToString();

            if (date <= 0 || dateStr.Length != 8)
                return dateStr;

            if (chnType)
                return dateStr.Substring(0, 4) + "年" + dateStr.Substring(4, 2) + "月" + dateStr.Substring(6) + "日";
            return dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6);
        }

        /// <summary>
        /// 计算工作日期，1.1、5.1、10.1
        /// </summary>
        /// <param name="BeginDate">开始日期</param>
        /// <param name="numday">工作日</param>
        /// <returns></returns>
        public static string GetWorkDate(DateTime BeginDate, int numday)
        {
            DateTime date;
            bool IsBegin = true;//作为开始标识,判断循环是否是第一次循环
                                //五一假期
            DateTime date51 = new DateTime(BeginDate.Year, 5, 1);
            DateTime date53 = new DateTime(BeginDate.Year, 5, 3);
            //十一假期
            DateTime date101 = new DateTime(BeginDate.Year, 10, 1);
            DateTime date107 = new DateTime(BeginDate.Year, 10, 7);
            //元旦假期
            DateTime date11 = new DateTime(BeginDate.Year, 1, 1);
            DateTime date13 = new DateTime(BeginDate.Year, 1, 3);
            for (int i = 0; i < Math.Abs(numday);)
            {
                if (IsBegin != true)
                {
                    BeginDate = BeginDate.AddDays(1);
                }
                IsBegin = false;//循环完第一次后，就不在启用
                date = new DateTime(BeginDate.Year, BeginDate.Month, BeginDate.Day);
                //判断五一假日
                if (BeginDate.CompareTo(date51) > 0 && BeginDate.CompareTo(date53) < 0)
                {
                    continue;
                }
                //判断十一假日
                if (BeginDate.CompareTo(date101) > 0 && BeginDate.CompareTo(date107) < 0)
                {
                    continue;
                }
                //判断元旦假期
                if (BeginDate.CompareTo(date11) > 0 && BeginDate.CompareTo(date13) < 0)
                {
                    continue;
                }

                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        break;
                    case DayOfWeek.Sunday:
                        break;
                    default:
                        i = i + 1;
                        break;
                }
            }
            return GetStandardDate(Convert.ToString(BeginDate));
        }

        /// <summary>
        /// 	Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "hours">The hours to be set.</param>
        /// <param name = "minutes">The minutes to be set.</param>
        /// <param name = "seconds">The seconds to be set.</param>
        /// <returns>The DateTime including the new time value</returns>
        public static DateTime SetTime(DateTime date, int hours, int minutes, int seconds)
        {
            return SetTime(date, new TimeSpan(hours, minutes, seconds));
        }

        /// <summary>
        /// 	Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <returns>
        /// 	The DateTime including the new time value
        /// </returns>
        public static DateTime SetTime(DateTime date, TimeSpan time)
        {
            return date.Date.Add(time);
        }

        /// <summary>
        ///     Indicates whether the source DateTime is before the supplied DateTime.
        /// </summary>
        /// <param name="source">The source DateTime.</param>
        /// <param name="other">The compared DateTime.</param>
        /// <returns>True if the source is before the other DateTime, False otherwise</returns>
        public static bool IsBefore(DateTime source, DateTime other)
        {
            return source.CompareTo(other) < 0;
        }

        /// <summary>
        ///     Indicates whether the source DateTime is before the supplied DateTime.
        /// </summary>
        /// <param name="source">The source DateTime.</param>
        /// <param name="other">The compared DateTime.</param>
        /// <returns>True if the source is before the other DateTime, False otherwise</returns>
        public static bool IsAfter(DateTime source, DateTime other)
        {
            return source.CompareTo(other) > 0;
        }

        /// <summary>
        /// 时间比对函数
        /// </summary>
        /// <param name="Interval">时间比对类型</param>
        /// <param name="StartDate">开始时间</param>
        /// <param name="EndDate">结束时间</param>
        /// <returns>两个事件的比对差值</returns>
        public static long DateDiff(DateInterval Interval, System.DateTime StartDate, System.DateTime EndDate)
        {
            long lngDateDiffValue = 0;
            System.TimeSpan TS = new System.TimeSpan(EndDate.Ticks - StartDate.Ticks);
            switch (Interval)
            {
                case DateInterval.Second:
                    lngDateDiffValue = (long)TS.TotalSeconds;
                    break;
                case DateInterval.Minute:
                    lngDateDiffValue = (long)TS.TotalMinutes;
                    break;
                case DateInterval.Hour:
                    lngDateDiffValue = (long)TS.TotalHours;
                    break;
                case DateInterval.Day:
                    lngDateDiffValue = (long)TS.Days;
                    break;
                case DateInterval.Week:
                    lngDateDiffValue = (long)(TS.Days / 7);
                    break;
                case DateInterval.Month:
                    lngDateDiffValue = (long)(TS.Days / 30);
                    break;
                case DateInterval.Quarter:
                    lngDateDiffValue = (long)((TS.Days / 30) / 3);
                    break;
                case DateInterval.Year:
                    lngDateDiffValue = (long)(TS.Days / 365);
                    break;
            }
            return (lngDateDiffValue);
        }

        /// <summary>
        /// C#计算两个日期之间相差的天数
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static int DateDiff(DateTime dateStart, DateTime dateEnd)
        {
            DateTime start = Convert.ToDateTime(dateStart.ToShortDateString());
            DateTime end = Convert.ToDateTime(dateEnd.ToShortDateString());
            TimeSpan sp = end.Subtract(start);
            return sp.Days;
        }

        /// <summary>
        /// 时间戳转为本地时间
        /// </summary>
        /// <param name="timeStamp">10或13位时间戳</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromTimeStamp(string timeStamp)
        {
            if (timeStamp.Length != 10 && timeStamp.Length != 13)
                throw new ArgumentOutOfRangeException();
            long lTime = long.Parse(timeStamp);
            var dto = timeStamp.Length == 10 ? DateTimeOffset.FromUnixTimeSeconds(lTime) : DateTimeOffset.FromUnixTimeMilliseconds(lTime);
            return dto.ToLocalTime().DateTime;
            //DateTime dtStart = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            //long lTime = long.Parse(timeStamp + "0000000");
            //TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式,秒级
        /// </summary>
        /// <param name="time">默认当前时间</param>
        /// <returns></returns>
        public static int GetTimeStampInt(System.DateTime? time = null)
        {
            if (time == null)
                time = DateTime.Now;
            //System.DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new System.DateTime(1970, 1, 1));
            return (int)(time.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式,毫秒级
        /// </summary>
        /// <param name="time">默认当前时间</param>
        /// <returns></returns>
        public static long GetTimeStampLong(System.DateTime? time = null)
        {
            if (time == null)
                time = DateTime.Now;
            //System.DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new System.DateTime(1970, 1, 1));
            return (long)(time.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 是否默认时间或者最小时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsNull(System.DateTime time)
        {
            if (time == DateTime.MinValue || time == default(DateTime))
                return true;
            if (time == BaseDateTime)
                return true;
            return false;
        }
        /// <summary>
        /// 1901/01/01
        /// </summary>
        public static DateTime BaseDateTime
        {
            get
            {
                return new DateTime(1901, 1, 1);
            }
        }
    }
}