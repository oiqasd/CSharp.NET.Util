using CSharp.Net.Util;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                LogHelper.Fatal(ex);
            }
        }
    }
}
