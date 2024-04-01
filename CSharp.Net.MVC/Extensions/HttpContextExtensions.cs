using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc;

public static class HttpContextExtensions
{
    //public static void AddHttpContextAccessor(this IServiceCollection services)
    //{
    //    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    //}

    public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
    {
        var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        HttpContextExt.Configure(httpContextAccessor);
        return app;
    }

    /// <summary>
    /// 获取客户端ip
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string GetRemoteIP(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();
    }

    /// <summary>
    /// 获取完整请求地址
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetRequestUrl(this HttpRequest request)
    {
        return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
    }

    /// <summary>
    /// 获取本机ip
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string GetLocalIP(this HttpContext httpContext)
    {
        return httpContext.Connection.LocalIpAddress?.MapToIPv4()?.ToString();
    }

    /// <summary>
    /// 获取 Action 特性
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static TAttribute GetMetadata<TAttribute>(this HttpContext httpContext)
        where TAttribute : class
    {
        return httpContext.GetEndpoint()?.Metadata?.GetMetadata<TAttribute>();
    }

    /// <summary>
    /// 获取 控制器/Action 描述器
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static ControllerActionDescriptor GetControllerActionDescriptor(this HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata?.FirstOrDefault(u => u is ControllerActionDescriptor) as ControllerActionDescriptor;
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
        return httpContext.Request.Cookies[Name];
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
        CookieOptions option = new CookieOptions();
        option.Path = Path;
        option.Domain = Domain;
        if (EndTime.HasValue)
        {
            option.Expires = EndTime.Value;
        }
        httpContext.Response.Cookies.Append(Name, Value, option);

    }
    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="Name"></param>
    /// <param name="Value"></param>
    /// <param name="EndTime">到期时间</param>
    public static void SetCookie(this HttpContext httpContext, string Name, string Value, DateTime? EndTime)
    {
        CookieOptions option = new CookieOptions();
        if (EndTime.HasValue)
        {
            option.Expires = EndTime.Value;
        }
        httpContext.Response.Cookies.Append(Name, Value, option);
    }

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="Name"></param>
    /// <param name="Value"></param>
    /// <param name="expiresSeconds">保存秒数</param>
    public static void SetCookie(this HttpContext httpContext, string Name, string Value, int expiredSeconds = 0)
    {
        CookieOptions option = new CookieOptions();
        if (expiredSeconds > 0)
        {
            option.Expires = DateTime.Now.AddSeconds(expiredSeconds);
        }
        httpContext.Response.Cookies.Append(Name, Value, option);
    }

    /// <summary>
    /// sign in use <see cref="ClaimsIdentity"/>
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="authenticationType"></param>
    /// <param name="claims">this key suggest use <c>ClaimTypes</c><see cref="ClaimTypes"/></param>
    /// <param name="expiressTime"></param>
    public static async Task SignInAsync(this HttpContext httpContext, string authenticationType, Dictionary<string, string> claims, DateTime? expiressTime = null)
    {
        if (claims == null || claims.Count <= 0) throw new ArgumentException("claims dic can't be null.");
        var identity = new ClaimsIdentity(authenticationType);
        foreach (var a in claims)
        {
            //identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, ""));
            identity.AddClaim(new Claim(a.Key, a.Value));
        }
        var clapro = new ClaimsPrincipal(identity);
        await httpContext.SignInAsync(ClaimKit._scheme, clapro, new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = expiressTime ?? DateTime.Now.AddYears(10) });
    }

    /// <summary>
    /// sign out claims
    /// </summary>
    /// <param name="httpContext">上下文</param>
    public static async Task SignOutAsync(this HttpContext httpContext)
    {
        await httpContext.SignOutAsync(ClaimKit._scheme);
    }

    /// <summary>
    /// 是否登录
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static bool IsSignIn(this HttpContext httpContext)
    {
        return httpContext.User.Identity.IsAuthenticated;
    }

    #endregion
}