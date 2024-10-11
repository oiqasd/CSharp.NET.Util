using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 管道中处理特殊字符串
    /// <code>app.UseMiddleware<RawJsonMiddleware>();</code>
    /// </summary>
    public class RawJsonMiddleware
    {
        private readonly RequestDelegate _next;
        public RawJsonMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            // 检查是否是 JSON 请求
            if (HttpMethods.IsPost(request.Method) &&
                request.ContentType != null &&
                request.ContentType.Contains("application/json"))
            {
                //允许多次读取
                //request.EnableBuffering();
                using (var reader = new StreamReader(request.BodyReader.AsStream(), Encoding.UTF8, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    if (body.IsHasValue() && body.Contains("\n"))
                    {
                        body = body.Replace("\n", "\\n");
                        var bytes = Encoding.UTF8.GetBytes(body);
                        using (var requestBody = new MemoryStream())
                        {
                            //requestBody.Seek(0, SeekOrigin.Begin);
                            //把修改写入流中
                            requestBody.Write(bytes, 0, bytes.Length);
                            request.Body = requestBody;
                            request.Body.Seek(0, SeekOrigin.Begin);
                            await _next(context);
                            return;
                        }
                    }
                    /** // 在管道的后续阶段，使用原始的请求体
                           var originalBodyStream = new MemoryStream();
                           requestBody.CopyTo(originalBodyStream);
                           originalBodyStream.Seek(0, SeekOrigin.Begin);
                           context.Response.OnStarting(() =>
                           {
                               // 在响应开始时，将原始请求体复制回去
                               requestBody.CopyTo(originalBody);
                               request.Body = originalBody;
                               return Task.CompletedTask;
                           });
                           */
                }
            }

            await _next(context);
        }
    }
}
