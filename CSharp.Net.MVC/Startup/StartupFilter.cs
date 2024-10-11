﻿using CSharp.Net.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace CSharp.Net.Mvc
{
    public class StartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                /**
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500; // Internal Server Error  
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        LogHelper.Error(exceptionHandlerPathFeature.Error, "Unhandled exception.");
                        // Respond with error details or a generic message  
                    });
                });
                */
                app.UseMiddleware<ExceptionHandlingMiddleware>();

                App.RootServices = app.ApplicationServices;
                //app.UseStaticHttpContext();
                app.EnableBuffering();
                //app.UseHttpLogging();
                app.UseW3CLogging();
                // 配置跨域
                app.UseCors("AllowCorsAny");
                app.Use(next =>
                {
                    return async (context) =>
                    {
                        try
                        {
                            await context.Request.EnableRewindAsync().ConfigureAwait(false);
                            await next(context);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError(ex.GetExcetionMessage());
                        }
                    };
                });
                //app.Use(next => new RequestDelegate(async context =>{await next(context);}));
                //app.Use(next => async (context) =>{await next(context);});
                //app.Use(async (context, next) => { await next(); });
                //没有释放会造成内存泄漏
                //AutofacUtil.Container = app.ApplicationServices.GetAutofacRoot();
                app.UseStaticFiles();
                app.UseRouting();
                if (App.Configuration["CMVC:FileSv"].ToBoolean())
                    app.UseFileSvUI();
                if (App.Configuration["CMVC:ApiDoc"].ToBoolean())
                    app.UseApiDocument(virPath: App.Configuration["CMVC:VirPath"]);

                {
                    /*
                    //负载均衡器转发获取真实ip配置 亲测不要也能获取X-Forwarded-For
                    //https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-5.0
                    app.UseForwardedHeaders(new ForwardedHeadersOptions
                    {
                        ForwardedHeaders = ForwardedHeaders.All,
                        //配置代理的IP地址
                        KnownNetworks = { new IPNetwork(IPAddress.Parse("172.17.0.0"), 16) }
                        //KnownProxies = { IPAddress.Parse("172.17.0.0") }
                    });

                    app.UseForwardedHeaders(new ForwardedHeadersOptions
                    {
                        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                    });
                    */

                    /*
                     * 编辑Nginx配置文件
                     * vim /etc/nginx/sites-available/default
                     * Esc进入命令模式，gg跳至首行，然后dG，清空当前配置
                     * server {
                        listen        80;
                        server_name   example.com *.example.com;
                        location / {
                            proxy_pass         http://127.0.0.1:5000;
                            proxy_http_version 1.1;
                            proxy_set_header   Upgrade $http_upgrade;
                            proxy_set_header   Connection keep-alive;
                            proxy_set_header   Host $host;
                            proxy_cache_bypass $http_upgrade;
                            proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
                            proxy_set_header   X-Forwarded-Proto $scheme;
                            }
                        }
                     * 
                     * 保存退出 Esc + :wq
                     * 测试配置是否正确 sudo nginx -t
                     * Nginx重新加载配置 sudo nginx -s reload
                     */
                }
                //app.UseMiddleware<RawJsonMiddleware>();
                app.Map("/health", (_map) =>
                {
                    _map.Run(async (context) =>
                    {
                        await context.Response.WriteAsync("ok");
                    });
                });
                app.Map("/runtime", (_map) =>
                {
                    _map.Run(async (context) =>
                    {
                        var ri = AppDomainHelper.PrintRuntimeInfo();
                        await context.Response.WriteAsync(ri, Encoding.GetEncoding("GBK"));
                    });
                });

                //app.UseEndpoints(endpoints =>
                //{
                //    endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { });
                //});

                //app.UseHealthChecks();

                /*
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }
                app.UseResponseCaching();
                */

                // 配置权限验证
                //app.UseAuthorization();
                //app.UseAuthentication();

                //app.UseMiddleware<AuthHelper>();

                // 配置全局路由
                app.UseMvc(route =>
                {
                    route.MapRoute(name: "default", template: "{controller}/{action}");
                });
                //app.UseHttpsRedirection();
                //app.UseMvc();
            };
        }
    }
}
