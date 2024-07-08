using CSharp.Net.Mvc.Filters;
using CSharp.Net.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: HostingStartup(typeof(CSharp.Net.Mvc.HostingStartup))]
namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 无入侵：
    /// 0、launchSettings.json中添加环境变量environmentVariables:{ASPNETCORE_HOSTINGSTARTUPASSEMBLIES:""}
    /// 1、wondows:环境变量
    /// 2、Linux:export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=xxxx
    /// 3、Linux永久设置,在/etc/profile 或者用户的 ~/.bashrc 文件中添加上述命令
    /// 4、Docker-Dockerfile: ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES xxxx
    /// 5、Docker-Compose: services:appxx:environment:ASPNETCORE_HOSTINGSTARTUPASSEMBLIES xxxx
    /// 代码修改：
    /// 6、Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "CSharp.Net.Mvc");
    /// </summary>
    public class HostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {            
            //装载配置
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
                App.Configuration = config.Build();
            });
            //builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
            //优先级:使用命令行 > UseUrls > json配置文件 > launchSettings applicationUrl
            //builder.UseUrls("http://*:5000");
            builder.UseIISIntegration();
            builder.UseKestrel(o =>
            {
                o.Limits.MaxRequestBodySize = null;
                //限制每个 HTTP/2 连接的并发请求流的数量
                o.Limits.Http2.MaxStreamsPerConnection = 100;
            });

            builder.ConfigureLogging((context, loggingBuilder) =>
            {
                //禁用控制台字体颜色输出
                //loggingBuilder.ClearProviders().AddConsole(option => { option.DisableColors = true; });
                loggingBuilder.ClearProviders().AddSimpleConsole(o => { o.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled; });
                // 过滤掉System和Microsoft开头的命名空间下的组件产生的警告级别日志
                //loggingBuilder.AddFilter("System", LogLevel.Warning); 
                //loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);
                loggingBuilder.AddFilter(level => level != Microsoft.Extensions.Logging.LogLevel.Warning);
                loggingBuilder.AddLog4Net("log4net.config");
            });

            builder.ConfigureServices(services =>
            {
                Trace.Listeners.Clear();
                Trace.AutoFlush = true;
                Trace.Listeners.Add(new TextWriterTraceListener(DateTime.Now.ToString("yyyyMMdd") + ".log"));

                //print UnobservedTaskException after gc
                TaskScheduler.UnobservedTaskException += (sender, e) =>
                    Trace.TraceError(e.Exception.GetExcetionMessage());

                services.AddRazorPages();
                //linux下无法使用gb2313编码时需要注册
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//System.Text.Encoding.CodePages

                // 配置跨域
                services.AddCors(options =>
                {
                    /*浏览器的同源策略，就是出于安全考虑，浏览器会限制从脚本发起的跨域HTTP请求（比如异步请求GET, POST, PUT, DELETE, OPTIONS等等，
                      所以浏览器会向所请求的服务器发起两次请求，第一次是浏览器使用OPTIONS方法发起一个预检请求，第二次才是真正的异步请求，
                      第一次的预检请求获知服务器是否允许该跨域请求：如果允许，才发起第二次真实的请求；如果不允许，则拦截第二次请求。
                      Access-Control-Max-Age用来指定本次预检请求的有效期，在此期间不用发出另一条预检请求。*/
                    var preflightMaxAge = new TimeSpan(0, 30, 0);

                    //跨域规则的名称
                    options.AddPolicy("AllowCorsAny", policy =>
                    {
                        // 设置跨域来源，多个用逗号隔开                  
                        var origins = App.Configuration["Cors:WithOrigins"]?.Split(',') ?? [];
                        origins.ToList().ForEach(f => { f = f.Trim(); });
                        policy.WithOrigins(origins)
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .AllowCredentials();
                        //policy.AllowAnyOrigin(); // 允许所有来源的主机访问
                        policy.SetIsOriginAllowed(s => true);
                        policy.SetPreflightMaxAge(preflightMaxAge);
                        policy.WithExposedHeaders("Content-Disposition");//下载文件时，文件名称会保存在headers的Content-Disposition属性里面
                    });
                });

                //控制器本身的实例及处理默认由框架创建和拥有，替换成容器
                //services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
                // 注册拦截器
                services.AddMvc(options =>
                {
                    // 全局异常捕获
                    options.Filters.Add<ExceptionHandle>();
                    //全局签名验证，可放controller上做局部验证
                    options.Filters.Add<PrivacyHandle>();
                    // options.Filters.Add(typeof(PreventResubmitActionFilter));
                    // options.Filters.Add(typeof(AuthorizeValidate));
                    options.RespectBrowserAcceptHeader = true;
                    options.EnableEndpointRouting = false;//使用不带终结点路由的MVC
                })
                .AddJsonOptions(option => JsonHelper.SetJsonSerializerOptions(option.JsonSerializerOptions));
                services.AddHttpContextAccessor();
                if (App.Configuration["CMVC:ApiDoc"].ToBoolean())
                    services.AddSwagger();

                services.AddW3CLogging(logging =>
                {
                    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.W3CLoggingFields.All;
                    logging.AdditionalRequestHeaders.Add("x-forwarded-for");
                    logging.AdditionalRequestHeaders.Add("x-client-ssl-protocol");
                    logging.FileSizeLimit = 5 * 1024 * 1024;
                    logging.RetainedFileCountLimit = 2;
                    logging.FileName = "W3CLog";
                    logging.LogDirectory = AppDomainHelper.GetRunRoot;
                    logging.FlushInterval = TimeSpan.FromSeconds(2);
                });
                services.AddSingleton<IStartupFilter, StartupFilter>();
                //services.AddScoped<FileSvController>();
            });
        }
    }
}
