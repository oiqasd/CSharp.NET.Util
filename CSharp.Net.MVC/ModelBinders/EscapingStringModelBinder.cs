using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 转义字符串模型绑定器
    /// <code>
    /// [ModelBinder(BinderType = typeof(EscapingStringModelBinder))]
    /// string data
    /// </code>
    /// </summary>
    public class EscapingStringModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;
            request.EnableBuffering();

            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                body = body.Replace("\n", "\\n");

                bindingContext.Result = ModelBindingResult.Success(body);
                request.Body.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
