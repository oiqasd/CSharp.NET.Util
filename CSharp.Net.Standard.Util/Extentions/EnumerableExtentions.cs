using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Net.Standard.Util.Extentions
{
    public static class EnumerableExtentions
    {
        /// <summary>
        /// 支持中文拼音
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="memberSelector"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Func<T, string> memberSelector)
        {
            var culture = new System.Globalization.CultureInfo("zh-cn");
            return source.ThenBy(memberSelector, StringComparer.Create(culture, true));
        }

        /// <summary>
        /// 支持中文thenbyDescending拼音
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="memberSelector"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, Func<T, string> memberSelector)
        {
            var culture = new System.Globalization.CultureInfo("zh-cn");
            return source.ThenByDescending(memberSelector, StringComparer.Create(culture, true));
        }

        /// <summary>
        /// 支持中文拼音正序排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="memberSelector"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByAscending<T>(this IEnumerable<T> source, Func<T, string> memberSelector)
        {
            var culture = new System.Globalization.CultureInfo("zh-cn");
            return source.OrderBy(memberSelector, StringComparer.Create(culture, true));
        }

        /// <summary>
        /// 支持中文拼音倒序排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="memberSelector"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, Func<T, string> memberSelector)
        {
            var culture = new System.Globalization.CultureInfo("zh-cn");
            return source.OrderByDescending(memberSelector, StringComparer.Create(culture, true));
        }

    }
}
