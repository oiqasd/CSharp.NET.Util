// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.AspNetCore.Swagger;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class SwaggerMiddleware
{

    public static void UseSwaggerMiddleware(this IApplicationBuilder app, string title = "ApiHelp V1 doc", string virPath = null)
    {
        app.UseMiddleware<SwaggerAuthMiddleware>();
        // 配置Swagger
        // 启用中间件服务生成Swagger作为JSON终结点
        app.UseSwagger();
        // 启用中间件服务对swagger-ui，指定Swagger JSON终结点
        app.UseSwaggerUI(c =>
        {
            //foreach (FieldInfo field in typeof(ApiVersionInfo).GetFields())
            //{
            //    c.SwaggerEndpoint($"/swagger/{field.Name}/swagger.json", $"{field.Name}");
            //}
            c.SwaggerEndpoint($"{virPath}/swagger/v1/swagger.json", title);
            c.InjectJavascript($"/swagger_translator.js");
        });

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
}