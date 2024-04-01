using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CSharp.Net.Mvc
{
    public static class EnableBuffer
    {
        /// <summary>
        /// 开启Body重复读取
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder EnableBuffering(this IApplicationBuilder app)
        {
            return app.Use(next => context =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });
        }
    }
}
