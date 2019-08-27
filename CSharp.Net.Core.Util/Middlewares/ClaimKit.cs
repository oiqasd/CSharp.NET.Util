using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Core.Util.Middlewares
{
    /// <summary>
    /// 缓存套件
    /// </summary>
    public static class ClaimKit
    {
        public static string _scheme { get { return _schemeTmp; } }
        private static string _schemeTmp { get; set; }
        /// <summary>
        /// 使用缓存
        /// 在ConfigureServices中使用
        /// 还需在Configure
        /// </summary>
        /// <param name="services"></param>
        /// <param name="loginPath">登陆地址.例:/Account/Login</param>
        /// <param name="accessDeniedPath">无权限访问地址.例:/Error/Forbidden</param>
        /// <param name="scheme"><see cref="CookieAuthenticationDefaults.AuthenticationScheme"/></param>
        /// <returns></returns>
        public static void AddAuthenticationUseClaims(this IServiceCollection services, string scheme = CookieAuthenticationDefaults.AuthenticationScheme, string loginPath = "/Account/Login", string accessDeniedPath = "/Error/Forbidden", string domain = null)
        {
            _schemeTmp = string.IsNullOrWhiteSpace(scheme) ? CookieAuthenticationDefaults.AuthenticationScheme : scheme;
            services.AddAuthentication(scheme).AddCookie(scheme, option =>
           {
               option.AccessDeniedPath = accessDeniedPath;
               option.LoginPath = loginPath;
               option.Cookie.Domain = domain;
               option.SlidingExpiration = true;
           });
            //将Cookie存储在服务端Redis缓存中
            //services.AddSingleton<ITicketStore, RedisTicketStore>();
            //services.AddOptions<CookieAuthenticationOptions>(scheme).Configure<ITicketStore>((option, ticket) => option.SessionStore = ticket);
        }

    }
    public class RedisTicketStore : ITicketStore
    {
        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            throw new NotImplementedException();
        }
    }
}
