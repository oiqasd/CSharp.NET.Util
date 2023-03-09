using CSharp.Net.Cache.Memory;
using CSharp.Net.Cache.Redis;
using CSharp.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class CSharpNetCacheServiceCollectionExtensions
{
    /// <summary>
    /// add redis cache
    /// </summary>
    /// <param name="services"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddStackExchangeRedisCache(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        services.AddOptions();
        services.Configure(setupAction);
        services.Add(ServiceDescriptor.Singleton<IRedisCache, RedisCacheProvider>());
        services.AddScoped<IHostedService, RedisPreHoldService>();

        return services;
    }

    /// <summary>
    /// add memory cache
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMemoryCache(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Add(ServiceDescriptor.Singleton<IMemoryCache, MemoryCacheProvider>());

        return services;
    }
}

class RedisPreHoldService : BackgroundService
{
    Timer timer = null;
    static RedisCacheOptions cacheOptions = null;
    public RedisPreHoldService(IRedisCache redisCache, IOptions<RedisCacheOptions> option)
    {
        redisCache.StringGet("_");
        if (option != null)
        {
            cacheOptions = option.Value;
        }
        else
        {
            cacheOptions = new RedisCacheOptions();
        }

        if (cacheOptions.RunThreadIntervalMilliseconds < 100) cacheOptions.RunThreadIntervalMilliseconds = 100;
        if (cacheOptions.MinWorkThread < 8) cacheOptions.MinWorkThread = 8;
        if (cacheOptions.MinIOThread < 1) cacheOptions.MinIOThread = 1;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
//#pragma warning disable CS4014
//        Task.Factory.StartNew(async () =>
//        {
//            while (true)
//            {
//                Console.WriteLine($"ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}");
//                AppDomainHelper.GetThreadPoolStats();

//                await Task.Delay(1000 * 3);
//            }
//        }, TaskCreationOptions.LongRunning);
//#pragma warning restore CS4014
//        return;

        if (!stoppingToken.IsCancellationRequested)
        {
            timer = new Timer(call =>
           {
               try
               {
                   if (cacheOptions.Environment == "dev")
                   {
                       Console.WriteLine($"{DateTime.Now.ToString(1)} CurrentThread ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}");
                   }

                   AppDomainHelper.GetThreadPoolStats(cacheOptions.MinWorkThread, cacheOptions.MinIOThread, cacheOptions.Environment);
               }
               catch (Exception ex)
               {
                   Console.WriteLine(ex.Message);
               }

           }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(cacheOptions.RunThreadIntervalMilliseconds));
        }

        await Task.CompletedTask;

    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        timer?.Dispose();
        await Task.CompletedTask;
    }
}

