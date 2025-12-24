// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Collections.Generic;

namespace CSharp.Net.Mvc;

public static class SwaggerMiddleware
{

    public static void UseApiDocument(this IApplicationBuilder app, string title = "ApiHelp V1 doc", string virPath = null)
    {
        app.UseMiddleware<SwaggerAuthMiddleware>();
  
        // 启用中间件服务生成Swagger作为JSON终结点
        app.UseSwagger(option => InjectDocServer(option, title, virPath));

        // 启用中间件服务对swagger-ui，指定Swagger JSON终结点
        app.UseSwaggerUI(c => InjectGroup(c, app, virPath));
        // Knife4j配置
        //app.UseKnife4UI(c =>
        //{
        //    c.RoutePrefix = ""; // serve the UI at root
        //    c.SwaggerEndpoint("/v1/api-docs", "ApiHelp V1");
        //});

        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllers(); //将本程序集定义的所有Controller和Action转换为一个个的EndPoint放到路由中间件的配置对象RouteOptions中 将EndpointMiddleware中间件注册到http管道中
        //    endpoints.MapSwagger("{documentName}/api-docs");
        //});
    }

    /// <summary>
    /// 配置中间服务
    /// </summary>
    /// <param name="option"></param>
    /// <param name="title"></param>
    /// <param name="virPath"></param>
    static void InjectDocServer(SwaggerOptions option, string title, string virPath)
    {
        //option.SerializeAsV2 = false;
        option.PreSerializeFilters.Add((s, r) =>
        {
            var servers = new List<OpenApiServer> {
                    new OpenApiServer { Url = $"{r.Scheme}://{r.Host.Value}/{virPath}", Description = title }
                };
            s.Servers = servers;
        });
    }

    /// <summary>
    /// 分组终结点
    /// </summary>
    /// <param name="options"></param>
    /// <param name="app"></param>
    /// <param name="virPath"></param>
    static void InjectGroup(SwaggerUIOptions options, IApplicationBuilder app, string virPath = null)
    {
        var apiDescriptionGroups = app.ApplicationServices.GetRequiredService<IApiDescriptionGroupCollectionProvider>().ApiDescriptionGroups.Items;
        foreach (var apidesc in apiDescriptionGroups)
        {
            options.SwaggerEndpoint($"{virPath}/swagger/{apidesc.GroupName ?? "default"}/swagger.json", apidesc.GroupName ?? "default");
        }

        //ui二级目录
        //options.RoutePrefix = virPath;
        //展开设置
        options.DocExpansion(DocExpansion.None);

        //foreach (FieldInfo field in typeof(ApiVersionInfo).GetFields())
        //    c.SwaggerEndpoint($"/swagger/{field.Name}/swagger.json", $"{field.Name}");

        //c.SwaggerEndpoint($"{virPath}/swagger/v1/swagger.json", title);
        //c.InjectJavascript($"/swagger_translator.js");
    }
}