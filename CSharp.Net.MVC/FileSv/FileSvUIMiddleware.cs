using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc.FileSv
{
    public class FileSvUIMiddleware
    {
        private readonly RequestDelegate _next;
        private const string EmbeddedFileNamespace = "";
        private readonly FilesvUIOptions _options;
        //private readonly StaticFileMiddleware _staticFileMiddleware;

        public FileSvUIMiddleware(RequestDelegate next, FilesvUIOptions options)
        {
            _next = next;
            _options = options ?? new FilesvUIOptions();
        }

        //public FileSvUIMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, ILoggerFactory loggerFactory, FilesvUIOptions options)
        //{
        //    _next= next;
        //    _options = options ?? new FilesvUIOptions();
        //    _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory, options);
        //}

        public async Task Invoke(HttpContext httpContext)
        {
            string method = httpContext.Request.Method;
            string value = httpContext.Request.Path.Value;

            if (!Regex.IsMatch(value, "^/?" + Regex.Escape(_options.RoutePrefix), RegexOptions.IgnoreCase))
            {
                await _next.Invoke(httpContext);
                return;
            }

            string key = App.Configuration["CMVC:AuthKey"];
            httpContext.Request.Cookies.TryGetValue(key, out string val);
            if (key.IsNotNullOrEmpty() && val != App.Configuration["CMVC:AuthValue"])
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            if (method == "GET" && Regex.IsMatch(value, "^/?" + Regex.Escape(_options.RoutePrefix) + "/?$", RegexOptions.IgnoreCase))
            {
                string location = ((string.IsNullOrEmpty(value) || value.EndsWith("/")) ? "index.html" : (value.Split('/').Last() + "/index.html"));
                RespondWithRedirect(httpContext.Response, location);
            }
            else if (!(method == "GET") || !Regex.IsMatch(value, "^/" + Regex.Escape(_options.RoutePrefix) + "/?index.html$", RegexOptions.IgnoreCase))
            {
                await _next.Invoke(httpContext);
            }
            else
            {
                await RespondWithIndexHtml(httpContext.Response);
            }
        }


        private StaticFileMiddleware CreateStaticFileMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, ILoggerFactory loggerFactory, FilesvUIOptions options)
        {
            StaticFileOptions options2 = new StaticFileOptions
            {
                RequestPath = (string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : ("/" + options.RoutePrefix)),
                FileProvider = new EmbeddedFileProvider(typeof(FileSvUIMiddleware).GetTypeInfo().Assembly, EmbeddedFileNamespace)
            };
            return new StaticFileMiddleware(next, hostingEnv, Options.Create(options2), loggerFactory);
        }

        private void RespondWithRedirect(HttpResponse response, string location)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = location;
        }

        private async Task RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html;charset=utf-8";
            using Stream stream = _options.IndexStream();
            StringBuilder stringBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
            foreach (KeyValuePair<string, string> indexArgument in GetIndexArguments())
            {
                stringBuilder.Replace(indexArgument.Key, indexArgument.Value);
            }
            await response.WriteAsync(stringBuilder.ToString(), Encoding.UTF8);
        }

        private IDictionary<string, string> GetIndexArguments()
        {
            return new Dictionary<string, string>
                {
                    { "$(Title)$", _options.Title }
                };
        }
    }
}
