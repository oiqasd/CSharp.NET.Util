using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class EnumerableExtension
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

    /// <summary>
    /// 	Performs an action for each item in the enumerable
    /// </summary>
    /// <typeparam name = "T">The enumerable data type</typeparam>
    /// <param name = "values">The data values.</param>
    /// <param name = "action">The action to be performed.</param>
    /// <example>
    /// 	var values = new[] { "1", "2", "3" };
    /// 	values.ConvertList&lt;string, int&gt;().ForEach(Console.WriteLine);
    /// </example>
    /// <remarks>
    /// 	This method was intended to return the passed values to provide method chaining. Howver due to defered execution the compiler would actually never run the entire code at all.
    /// </remarks>
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        foreach (var value in values)
            action(value);
    }

    /// <summary>
    /// Returns true if the <paramref name="source"/> is null or without any items.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return (source == null || !source.Any());
    }

    /// <summary>
    /// Returns true if the <paramref name="source"/> is contains at least one item.
    /// </summary>
    public static bool IsHasValue<T>(this IEnumerable<T> source)
    {
        return !source.IsNullOrEmpty();
    }
}
