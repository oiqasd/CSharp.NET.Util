using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Net.Standard.Util.Extentions
{
    /// <summary>
    /// Array 扩展方法
    /// </summary>
    public static class ArrayExtentions
    {/// <summary>
     /// 根据id生成 in 字符串
     /// 直接拼接Sql语句中（参数化需使用find_in_set函数，由于find_in_set函数效率低，不建议使用）
     /// </summary>
     /// <param name="ids">id集合</param>
     /// <param name="defaultValue">默认值（当ids值无效时，使用默认值）</param>
     /// <returns></returns>
        public static string IdsToInSql(this IList<long> ids, IList<long> defaultValue)
        {
            var result = @"''";
            if (ids != null && ids.Count > 0)
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
            if (ids != null && ids.Count > 0)
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
            if (ids != null && ids.Count > 0)
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
        /// 根据数组生成字符串("1,2,3,")
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>返回字符串</returns>
        public static string ToArrayIntoString<T>(this IList<T> array)
        {
            var result = string.Empty;
            if (array != null && array.Any())
            {
                foreach (var item in array)
                {
                    result += Convert.ToInt32(item) + ",";
                }
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
        /// 将数组转换成以、分隔的字符串（如1、2、3）
        /// </summary>
        /// <param name="list">list</param>
        /// <returns>字符串</returns>
        public static string IListToString(this IList<string> list)
        {
            StringBuilder str = new StringBuilder();
            if (list != null && list.Any())
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    str.Append($"{list[i]}");
                    if (i < list.Count() - 1)
                    {
                        str.Append("、");
                    }
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// 将数组转换成以,分隔的字符串（如1,2,3）
        /// </summary>
        /// <param name="list">list</param>
        /// <returns>字符串</returns>
        public static string IListToStringSplitWithComma(this IList<string> list)
        {
            StringBuilder str = new StringBuilder();
            if (list != null && list.Any())
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    str.Append($"'{list[i]}'");
                    if (i < list.Count() - 1)
                    {
                        str.Append(",");
                    }
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// 将数组转换成以选定字符串分隔的字符串（如1/2/3）
        /// </summary>
        /// <param name="list">list</param>
        /// <param name="value">选定字符串</param>
        /// <returns>字符串</returns>
        public static string IListToStringByString(this IList<string> list, string value)
        {
            StringBuilder str = new StringBuilder();
            if (list != null && list.Any())
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    str.Append($"{list[i]}");
                    if (i < list.Count() - 1)
                    {
                        str.Append(value);
                    }
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// 字符串分割
        /// </summary>
        /// <param name="value"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] Split(this string value, string separator)
        {
            return value.Split(separator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
