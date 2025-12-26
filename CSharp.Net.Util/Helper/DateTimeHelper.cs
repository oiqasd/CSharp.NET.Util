using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 枚举，比对时间的类型
    /// </summary>
    public enum DateInterval
    {
        /// <summary>
        /// 毫秒
        /// </summary>
        Milliseconds,
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
    public sealed class DateTimeHelper
    {
        /// <summary>
        /// 时间戳最大秒数
        /// 9999/12/31 23:59:59
        /// </summary>
        public static readonly long MaxUnixSeconds = 0x3afff4417fL; //253402300799;

        /// <summary>
        /// 默认时区转换
        /// 适用无明确时区的时间
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime ResolveZoneInfo(DateTime datetime)
        {
            TimeZoneInfo tzi = datetime.Kind == DateTimeKind.Unspecified ? TimeZoneInfo.Local : TimeZoneInfo.Utc;
            var converted = TimeZoneInfo.ConvertTime(datetime, tzi);
            return converted;
        }

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
                datetimestr = System.Convert.ToDateTime(datetimestr).ToString("yyyy-MM-dd").Replace("1900-01-01", replacestr);
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
        /// 判断字符串是否是时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsTime(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        /// <summary>
        /// 返回与当前时间相差的秒数
        /// </summary>
        /// <param name="time">计算的时间</param>
        /// <param name="Sec">计算的时间加上的指定秒数</param>
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
        /// 返回与当前时间相差的分钟数
        /// </summary>
        /// <param name="time">计算的时间</param>
        /// <param name="minutes">计算的时间加上的指定分钟数</param>
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
        /// 返回与当前时间相差的小时数
        /// </summary>
        /// <param name="time">计算的时间</param>
        /// <param name="hours">计算的时间加上的指定小时数</param>
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
        /// <param name="chnType">是否以中文年月日输出，默认否
        /// <para>是：xxxx年xx月xx日</para>
        /// <para>否：xxxx-xx-xx</para>
        /// </param>
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
            return GetStandardDate(System.Convert.ToString(BeginDate));
        }

        /// <summary>
        /// Sets the time on the specified DateTime value.
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
        /// Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <returns>
        /// The DateTime including the new time value
        /// </returns>
        public static DateTime SetTime(DateTime date, TimeSpan time)
        {
            return date.Date.Add(time);
        }

        /// <summary>
        /// Indicates whether the source DateTime is before the supplied DateTime.
        /// </summary>
        /// <param name="source">The source DateTime.</param>
        /// <param name="other">The compared DateTime.</param>
        /// <returns>True if the source is before the other DateTime, False otherwise</returns>
        public static bool IsBefore(DateTime source, DateTime other)
        {
            return source.CompareTo(other) < 0;
        }

        /// <summary>
        /// Indicates whether the source DateTime is before the supplied DateTime.
        /// </summary>
        /// <param name="source">The source DateTime.</param>
        /// <param name="other">The compared DateTime.</param>
        /// <returns>True if the source is before the other DateTime, False otherwise</returns>
        public static bool IsAfter(DateTime source, DateTime other)
        {
            return source.CompareTo(other) > 0;
        }

        /// <summary>
        /// <para>时间比对函数</para>
        /// <para>* 以秒为基准，年月日时分都基于秒换算</para>
        /// </summary>
        /// <param name="Interval">时间比对类型</param>
        /// <param name="StartDate">开始时间</param>
        /// <param name="EndDate">结束时间</param>
        /// <returns>两个时间的比对差值</returns>
        public static long DateDiff(DateInterval Interval, System.DateTime StartDate, System.DateTime EndDate)
        {
            long lngDateDiffValue = 0;
            System.TimeSpan TS = new System.TimeSpan(EndDate.Ticks - StartDate.Ticks);
            switch (Interval)
            {
                case DateInterval.Milliseconds:
                    lngDateDiffValue = (long)TS.TotalMilliseconds;
                    break;
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
                    lngDateDiffValue = TS.Days;
                    break;
                case DateInterval.Week:
                    lngDateDiffValue = TS.Days / 7;
                    break;
                case DateInterval.Month:
                    lngDateDiffValue = TS.Days / 30;
                    break;
                case DateInterval.Quarter:
                    lngDateDiffValue = (TS.Days / 30) / 3;
                    break;
                case DateInterval.Year:
                    lngDateDiffValue = TS.Days / 365;
                    break;
            }
            return lngDateDiffValue;
        }

        /// <summary>
        /// <para>C#计算两个日期之间相差的天数</para>
        /// <para>* 以日期为基准，不受时分秒影响</para>
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static int DateDiff(DateTime dateStart, DateTime dateEnd)
        {
            DateTime start = System.Convert.ToDateTime(dateStart.ToShortDateString());
            DateTime end = System.Convert.ToDateTime(dateEnd.ToShortDateString());
            TimeSpan sp = end.Subtract(start);
            return sp.Days;
        }

        /// <summary>
        /// 时间戳转为本地时间
        /// </summary>
        /// <param name="timeStamp">时间戳(秒或者毫秒)
        /// <para>默认秒,大于 <see cref="DateTimeHelper.MaxUnixSeconds"/> 自动转换成毫秒</para></param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromTimeStamp(string timeStamp)
        {
            if (timeStamp.IsNullOrEmpty())
                return DateTime.MinValue;

            //if (timeStamp.Length != 10 && timeStamp.Length != 13)
            if (!long.TryParse(timeStamp, out long val))
                //throw new ArgumentException($"Could not parse String '{timeStamp}' to DateTime.");
                return DateTime.MinValue;

            var dto = val > MaxUnixSeconds ? DateTimeOffset.FromUnixTimeMilliseconds(val) : DateTimeOffset.FromUnixTimeSeconds(val);
            return dto.ToLocalTime().DateTime;
            //DateTime dtStart = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            //long lTime = long.Parse(timeStamp + "0000000");
            //TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        }

        /// <summary>
        /// 时间戳转为本地时间
        /// </summary>
        /// <param name="timeStamp">时间戳(秒或者毫秒)
        /// <para>默认秒,大于 <see cref="DateTimeHelper.MaxUnixSeconds"/> 自动转换成毫秒</para></param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromTimeStamp(long timeStamp)
        {
            var dto = timeStamp > MaxUnixSeconds ? DateTimeOffset.FromUnixTimeMilliseconds(timeStamp) : DateTimeOffset.FromUnixTimeSeconds(timeStamp);
            return dto.ToLocalTime().DateTime;
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式,秒级
        /// </summary>
        /// <param name="time">默认当前时间</param>
        /// <returns>在2038年将会溢出</returns>
        [Obsolete]
        public static int GetTimeStampInt(DateTime? time = null)
        {
            if (time == null)
                return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            DateTimeOffset localtime = DateTime.SpecifyKind(time.Value, DateTimeKind.Local);
            return (int)localtime.ToUnixTimeSeconds();
            //System.DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new System.DateTime(1970, 1, 1));
            //return (int)(time.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">默认当前时间</param>
        /// <param name="milliseconds">默认true:毫秒，fasle:秒</param>
        /// <returns>时间戳</returns>
        public static long GetTimestamp(DateTime? time, bool milliseconds = true)
        {
            DateTimeOffset localtime;
            if (time.HasValue)
                localtime = DateTime.SpecifyKind(time.Value, DateTimeKind.Local);
            else
                localtime = DateTimeOffset.UtcNow;

            if (milliseconds)
                return localtime.ToUnixTimeMilliseconds();

            return localtime.ToUnixTimeSeconds();
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="milliseconds">默认true:毫秒，fasle:秒</param>
        /// <returns>时间戳</returns>
        public static long GetTimestamp(bool milliseconds = true)
            => GetTimestamp(null, milliseconds);


        /// <summary>
        /// 是否默认时间或者最小时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsNull(DateTime time)
        {
            if (time == DateTime.MinValue || time == default)
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

        /// <summary>
        /// 当前月第一天
        /// </summary>
        /// <param name="date">默认当前月</param>
        /// <returns></returns>
        public static DateTime GetMonthFirstDay(DateTime date = default)
        {
            if (date == default) date = DateTime.Now;
            // DateTime.Parse($"{date.Value.Year}/{date.Value.Month}/01 00:00:00");
            // dt.AddDays(1 - dt.Day);
            var dt = new DateTime(date.Year, date.Month, 1);
            return dt;
        }

        /// <summary>
        /// 当前周第一天
        /// </summary>
        /// <param name="date">指定日期所在周的第一天 默认当前周</param>
        /// <param name="dayOfWeek">null:获取当前系统区域的默认周起始日（如中国默认周一，美国默认周日）</param>
        /// <returns></returns>
        public static DateTime GetWeekFirstDay(DateTime date = default, DayOfWeek? dayOfWeek = null)
        {
            if (date == default) date = DateTime.Now;
            if (dayOfWeek == null) dayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            int offset = date.DayOfWeek - dayOfWeek.Value;
            // 若偏移量为负数，说明当前日期在起始日之前，需加7天补全一周
            if (offset < 0)
            {
                offset += 7;
            }
            return date.AddDays(-offset);
        }

        /// <summary>
        /// 当前月最后一天
        /// </summary>
        /// <param name="date">默认当前月</param>
        /// <returns></returns>
        public static DateTime GetMonthLastDay(DateTime date = default)
        {
            return GetMonthFirstDay(date).AddMonths(1).AddSeconds(-1);
        }

        /// <summary>
        /// 日期格式转换
        /// <para>支持常规日期格式、yyyy-MM-dd HH:mm:ss、yyyy-MM-dd'T'HH:mm:sszzz、时间戳</para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format">自定义格式</param>
        /// <returns></returns>
        public static DateTime Parse(string value, string format = null)
        {
            if (value.IsNullOrEmpty()) throw new ArgumentNullException("value");
            if (format.IsNotNullOrEmpty() &&
                DateTime.TryParseExact(value, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime d))
                return d;
            if (DateTime.TryParse(value, out d))
                return d;
            if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
                return d;
            if (DateTime.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:sszzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
                return d;
            return GetDateTimeFromTimeStamp(value);
            throw new FormatException($"Could not parse String '{value}' to DateTime.");
        }

        /// <summary>
        /// 时间戳转换本地时间
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="isSecond">是否秒转化，default: auto select, true: second, false: millisecond </param>
        /// <returns></returns>
        public static DateTime Parse(long timestamp, bool? isSecond = null)
        {
            if (timestamp < 0) throw new ArgumentNullException("timestamp");
            if (isSecond == null)
                return GetDateTimeFromTimeStamp(timestamp);

            DateTime dateTime;
            if (isSecond.Value)
                dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
            else
                dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
            return dateTime;
        }

        /// <summary>
        /// 将 DateTime 转换成 DateTimeOffset
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToDateTimeOffset(DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }

        /// <summary>
        /// 将 DateTimeOffset 转换成本地 DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
                return dateTime.UtcDateTime;
            if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
                return dateTime.ToLocalTime().DateTime;
            else
                return dateTime.DateTime;
        }

        /// <summary>
        /// 获取当前第几周
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static int GetWeekOfYear(DateTime dateTime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday, CultureInfo cultureInfo = null)
        {
            cultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;
            var i = cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, firstDayOfWeek);
            return i;
        }
        /// <summary>
        /// 检查时间戳是否过期
        /// 默认检查30s
        /// </summary>
        /// <returns></returns>
        public static bool CheckTimeStamp(long timeStamp, int intervalSeconds = 30)
        {
            long nowTimeStamp = GetTimestamp();
            if (Math.Abs(nowTimeStamp - timeStamp) > intervalSeconds * 1000)
                return false;
            return true;
        }

        public static long DaysToHours(long timePeriodDays) =>
                timePeriodDays * 0x18L;
        public static long DaysToMilliseconds(long timePeriodDays) =>
                SecondsToMilliseconds(DaysToSeconds(timePeriodDays));
        public static long DaysToSeconds(long timePeriodDays) =>
                MinutesToSeconds(DaysToMinutes(timePeriodDays));
        public static long DaysToMinutes(long timePeriodDays) =>
                HoursToMinutes(DaysToHours(timePeriodDays));
        public static long MinutesToSeconds(long timePeriodMinutes) =>
                timePeriodMinutes * 60L;
        public static long HoursToMinutes(long timePeriodHours) =>
                timePeriodHours * 60;
        public static long SecondsToMilliseconds(long timePeriodSeconds) =>
                timePeriodSeconds * 0x3e8L;
    }
}