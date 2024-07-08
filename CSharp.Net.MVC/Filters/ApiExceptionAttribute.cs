using CSharp.Net.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.Filters
{
    /// <summary>
    /// api异常处理
    /// </summary>
    public class ApiExceptionAttribute : ExceptionFilterAttribute
    {
        ILogger<ApiExceptionAttribute> _logger;
        public ApiExceptionAttribute(ILogger<ApiExceptionAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Result == null)
            {
                if (context.Exception is AppException)
                {
                    var error = context.Exception as AppException;
                    Response<object> responseObj = new Response<object>((ReturnCode)error.ErrorCode, error.Message, error.Data);
                    context.Result = new ObjectResult(responseObj);
                }
                else if (context.Exception is NotImplementedException)
                {
                    context.HttpContext.Response.StatusCode = System.Net.HttpStatusCode.NotImplemented.GetHashCode();
                    Response responseObj = new Response(ReturnCode.NotImplemented);
                    context.Result = new ObjectResult(responseObj);
                }
                else
                {
                    base.OnException(context);
                    return;
                    //context.HttpContext.Response.StatusCode = System.Net.HttpStatusCode.InternalServerError.GetHashCode();
                    //context.Result = new ObjectResult(new Response(ErrorCode.Unknown, context.Exception.Message));
                }

                var client_ip = context.HttpContext.GetRemoteIP();
                var method_name = context.Exception.TargetSite.Name;
                var strLog = $"[IP:{client_ip}],[接口:{context.HttpContext.Request.Path} 链：{method_name}],[错误信息:{context.Exception.Message}]";
                context.ExceptionHandled = true;

                Task.Run(() => _logger.LogInformation(strLog));
                return;
            }
            base.OnException(context);
        }
    }
}
