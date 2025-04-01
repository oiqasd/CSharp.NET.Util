using CSharp.Net.Mvc.FileOS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharp.Net.Mvc
{
    public static class FileOSUIBuilderExtensions
    {
        public static IApplicationBuilder UseFileOSUI(this IApplicationBuilder app, Action<FileOSOptions> setupAction = null)
        {
            FileOSOptions options = new FileOSOptions();
            if (setupAction != null)
            {
                setupAction(options);
            }
            else
            {
                options = app.ApplicationServices.GetRequiredService<IOptions<FileOSOptions>>().Value;
            }

            app.UseMiddleware<FileOSUIMiddleware>(new object[1] { options });
  
            return app;
        }
    }
}
