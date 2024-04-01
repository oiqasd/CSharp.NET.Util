using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharp.Net.Mvc
{
    public class App
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        static App()
        {

        }

        /// <summary>
        /// 存储根服务，可能为空
        /// </summary>
        public static IServiceProvider RootServices { get; set; }

        public static IConfiguration Configuration =>
            RootServices?.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();

        /// <summary>
        /// 获取请求上下文
        /// </summary>
        public static HttpContext HttpContext =>
            Try.Func(() => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext);

        /// <summary>
        /// 解析服务提供器
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(Type serviceType)
        {
            return RootServices.GetRequiredService<IServiceProvider>();
            //// 处理控制台应用程序
            //if (HostEnvironment == default) return RootServices;

            //// 第一选择，判断是否是单例注册且单例服务不为空，如果是直接返回根服务提供器
            //if (RootServices != null && InternalApp.InternalServices
            //.Where(u => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType))
            //.Any(u => u.Lifetime == ServiceLifetime.Singleton)) return RootServices;

            //// 第二选择是获取 HttpContext 对象的 RequestServices
            //var httpContext = HttpContext;
            //if (httpContext?.RequestServices != null) return httpContext.RequestServices;
            //// 第三选择，创建新的作用域并返回服务提供器
            //else if (RootServices != null)
            //{
            //    var scoped = RootServices.CreateScope();
            //    UnmanagedObjects.Add(scoped);
            //    return scoped.ServiceProvider;
            //}
            //// 第四选择，构建新的服务对象（性能最差）
            //else
            //{
            //    var serviceProvider = InternalApp.InternalServices.BuildServiceProvider();
            //    UnmanagedObjects.Add(serviceProvider);
            //    return serviceProvider;
            //}
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetService<TService>(IServiceProvider serviceProvider = default)
            where TService : class
        {
            return GetService(typeof(TService), serviceProvider) as TService;
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetService(Type type, IServiceProvider serviceProvider = default)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetService(type);
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetRequiredService<TService>(IServiceProvider serviceProvider = default)
            where TService : class
        {
            return GetRequiredService(typeof(TService), serviceProvider) as TService;
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetRequiredService(Type type, IServiceProvider serviceProvider = default)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetRequiredService(type);
        }

        /// <summary>
        /// 获取命令行配置
        /// </summary>
        /// <param name="args"></param>
        /// <param name="switchMappings"></param>
        /// <returns></returns>
        public static CommandLineConfigurationProvider GetCommandLineConfiguration(string[] args,
            IDictionary<string, string> switchMappings = null)
        {
            var commandLineConfiguration = new CommandLineConfigurationProvider(args, switchMappings);
            commandLineConfiguration.Load();

            return commandLineConfiguration;
        }

        /// <summary>
        /// 获取当前线程 Id
        /// </summary>
        /// <returns></returns>
        public static int GetThreadId()
        {
            return Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// 获取当前请求 TraceId
        /// </summary>
        /// <returns></returns>
        public static string GetTraceId()
        {
            return Activity.Current?.Id ?? (RootServices == null ? default : HttpContext?.TraceIdentifier);
        }

    }
}
