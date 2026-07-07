using Microsoft.Extensions.DependencyInjection;

namespace CSharp.Net.Mvc;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="preflightMaxAge">default 1h</param>
    public static void AddMyCors(IServiceCollection services, TimeSpan? preflightMaxAge = null)
    {

        services.AddCors(options =>
        {
            options.AddPolicy("AllowCorsAny", policy =>
            {
                policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                policy.SetPreflightMaxAge(preflightMaxAge ?? TimeSpan.FromHours(1));
            });

        });
    }
}
