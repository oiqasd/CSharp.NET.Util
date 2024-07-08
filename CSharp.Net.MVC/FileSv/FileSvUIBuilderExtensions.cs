using CSharp.Net.Mvc.FileSv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharp.Net.Mvc
{
    public static class FileSvUIBuilderExtensions
    {
        public static IApplicationBuilder UseFileSvUI(this IApplicationBuilder app, Action<FilesvUIOptions> setupAction = null)
        {
            FilesvUIOptions options = new FilesvUIOptions();
            if (setupAction != null)
            {
                setupAction(options);
            }
            else
            {
                options = app.ApplicationServices.GetRequiredService<IOptions<FilesvUIOptions>>().Value;
            }

            app.UseMiddleware<FileSvUIMiddleware>(new object[1] { options });
  
            return app;
        }
    }
}
