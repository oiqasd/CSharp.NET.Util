using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public static class HttpRequestExtension
{
    /// <summary>
    /// 设置请求进入时的两个请求头
    /// </summary>
    /// <param name="request"></param>
    public static void BeginAction(this WebRequest request)
    {
        if (request != null && request.Headers != null)
        {
            request.Headers["DateTimeF"] = DateTimeHelper.GetTimeStamp(DateTime.Now).ToString();
            request.Headers["GUID"] = Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// 获取请求进入时的时间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static long GetBeginTime(this WebRequest request)
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
    public static string GetID(this WebRequest request)
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
