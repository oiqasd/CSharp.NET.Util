using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

public static class HttpContextExtensions
{
    public static void AddHttpContextAccessor(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
    {
        var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        HttpContextExt.Configure(httpContextAccessor);
        return app;
    }

    public static string GetClientIP(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress.ToString();
    }

    #region Cookie Helper
    /// <summary>
    /// 获取Cookie
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="Name"></param>
    /// <returns></returns>
    public static string GetCookie(this HttpContext httpContext, string Name)
    {
        string s = "";
        try
        {
            s = httpContext.Request.Cookies[Name];
        }
        catch (Exception ex)
        {
        }
        return s;
    }

    /// <summary>
    /// 写Cookie
    /// </summary>
    /// <param name="Name">名称</param>
    /// <param name="Value">值</param>
    /// <param name="Path">路径</param>
    /// <param name="Domain">域</param>
    /// <param name="EndTime">失效日期</param>
    public static void SetCookie(this HttpContext httpContext, string Name, string Value, string Path, string Domain, DateTime? EndTime)
    {
        try
        {
            CookieOptions option = new CookieOptions();
            option.Path = Path;
            option.Domain = Domain;
            if (EndTime.HasValue)
            {
                option.Expires = EndTime.Value;
            }
            httpContext.Response.Cookies.Append(Name, Value, option);
        }
        catch (Exception ex)
        {
        }
    }

    #endregion
}
