using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Net.Util
{
    public class UriHelper
    {
        /// <summary>
        /// 返回 URL 中的查询参数，忽略大小写
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Dictionary<string, string> GetParams(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Invalid URL format.", url);
            }

            var query = uri.Query.TrimStart('?')
            .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split(new[] { '=' }, 2))
            .ToDictionary(
                part => Uri.UnescapeDataString(part[0]),
                part => part.Length > 1 ? Uri.UnescapeDataString(part[1]) : string.Empty,
                StringComparer.OrdinalIgnoreCase);

            return query;
        }

        /// <summary>
        /// 生成带查询参数的 URL，忽略大小写
        /// </summary>
        /// <param name="url"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string BuilderUri(string url, Dictionary<string, string> query)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Invalid URL format.", url);
            }

            var builder = new UriBuilder(uri)
            {
                Query = string.Join("&", query.Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"))
            };
            return builder.Uri.ToString();
        }
    }
}
