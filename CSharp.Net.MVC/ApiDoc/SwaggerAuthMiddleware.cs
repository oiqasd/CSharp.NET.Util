// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc;

public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate next;
    IConfiguration _configuration;
    public SwaggerAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            //var encodeU = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
            //var decodeU = Encoding.UTF8.GetString(Convert.FromBase64String(encodeU));

            string key = App.Configuration["CMVC:AuthKey"];
            if (key.IsNullOrEmpty())
            {
                await next.Invoke(context);
                return;
            }

            context.Request.Cookies.TryGetValue(key, out string val);
            if (val.IsHasValue() && val == _configuration["CMVC:AuthValue"])
            {
                await next.Invoke(context);
                return;
            }
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        else
        {
            await next.Invoke(context);
        }
    }

    public bool IsAuthrized(string name, string pwd)
    {
        return name.Equals("admin", StringComparison.InvariantCultureIgnoreCase) && pwd.Equals("123456");
    }

    public bool IsLocalRequest(HttpContext context)
    {
        if (context.Connection.RemoteIpAddress == null && context.Connection.LocalIpAddress == null)
            return true;

        if (context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress))
            return true;

        if (IPAddress.IsLoopback(context.Connection.RemoteIpAddress)) return true;

        return false;
    }
}
