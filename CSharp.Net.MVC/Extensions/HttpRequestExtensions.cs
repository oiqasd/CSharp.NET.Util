// ****************************************************
// * 创建日期：2023-7-17
// * 创建人：y
// * 备注：获取http请求内容
// ****************************************************

using CSharp.Net.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc;

public static class HttpRequestExtensions
{
    /// <summary>
    /// 读取请求数据，暂不支持文件读取
    /// get请求取Url
    /// post请求取body或者form
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>> ReadDataAsync(this HttpRequest request)
    {
        if (request == null) return null;
        if (request.Method == HttpMethods.Get)
        {
            var query = request.QueryString.ToString();
            return RegexHelper.QueryStringToDictionany(query);
        }

        if (request.Method == HttpMethods.Post && request.ContentType.Contains("json"))
        {
            var str = await ReadBodyAsync(request);
            return JsonHelper.GetJObject(str);
        }

        if (request.Method == HttpMethods.Post && request.ContentType.Contains("form-data"))
        {
            var form = await request.ReadFormAsync().ConfigureAwait(false);
            if (form == null) return null;
            var data = form.ToDictionary(p => p.Key, p => p.Value.ToString());
            return data;
        }
        return null;
    }

    /// <summary>
    /// 读取文件bytes[]
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<List<byte[]>> ReadFileBytesAsync(this HttpRequest request)
    {
        if (request == null) return null;
        if (request.Form.Files.Count == 0)
            return null;
        List<byte[]> list = new List<byte[]>();

        for (int i = 0; i < request.Form.Files.Count; i++)
        {
            using (Stream stream = request.Form.Files[i].OpenReadStream())
            {
                long len = request.Form.Files[i].Length;
                byte[] buffer = new byte[len];
                await stream.ReadAsync(buffer, 0, Convert.ToInt32(len)).ConfigureAwait(false);
                list.Add(buffer);
            }
        }
        return list;
    }

    /// <summary>
    /// 获取HttpRequest Body
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<string> ReadBodyAsync(this HttpRequest request)
    {
        if (request.Body != null && request.Body.Length > 0)
        {
            await request.EnableRewindAsync().ConfigureAwait(false);
            var encoding = request.GetEncoding();

            //var syncIOFeature = request.HttpContext.Features.Get<IHttpBodyControlFeature>();
            //if (syncIOFeature != null) syncIOFeature.AllowSynchronousIO = true;

            using (StreamReader sr = new StreamReader(request.Body, encoding, true, 1024, true))
            {
                var str = await sr.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
                return str;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取HttpRequest编码
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Encoding GetEncoding(this HttpRequest request)
    {
        var requestContentType = request.ContentType;
        var requestMediaType = requestContentType == null ? default(MediaType) : new MediaType(requestContentType);
        var requestEncoding = requestMediaType.Encoding;
        if (requestEncoding == null) requestEncoding = Encoding.UTF8;
        return requestEncoding;
    }

    public static async Task EnableRewindAsync(this HttpRequest request)
    {
        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
            //清空上一个节点的信息
            await request.Body.DrainAsync(CancellationToken.None);
        }
        if (request.Body.Position > 0)
            request.Body.Seek(0L, SeekOrigin.Begin);
    }

    /// <summary>
    /// 设置请求进入时的两个请求头
    /// </summary>
    /// <param name="request"></param>
    public static void BeginAction(this HttpRequest request)
    {
        if (request != null && request.Headers != null)
        {
            request.Headers["DateTimeF"] = DateTimeHelper.GetTimeStampLong(DateTime.Now).ToString();
            request.Headers["GUID"] = Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// 获取请求进入时的时间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static long GetBeginTime(this HttpRequest request)
    {
        if (request != null && request.Headers != null)
        {
            string st = request.Headers["DateTimeF"];
            if (st != null)
            {
                return ConvertHelper.TryPraseLong(st);
            }
        }
        return 0;
    }

    /// <summary>
    /// 获取请求进入时的此次请求唯一ID
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetID(this HttpRequest request)
    {
        if (request != null && request.Headers != null)
        {
            string st = request.Headers["GUID"];
            if (st != null)
            {
                return st;
            }
        }
        return "";
    }

}