using Microsoft.Extensions.DependencyInjection;

namespace CSharp.Net.Mvc;

public static class ServiceCollectionExtension
{
    public static void AddMyMvc(IServiceCollection services)
    {

        services.AddCors(options =>
        {
            options.AddPolicy("AllowCorsAny", policy =>
            {
                policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                policy.SetPreflightMaxAge(TimeSpan.FromHours(1));
            });

        });
    }
}
