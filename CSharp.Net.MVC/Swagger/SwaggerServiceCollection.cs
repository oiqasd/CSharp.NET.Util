using CSharp.Net.AspNetCore.Swagger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class SwaggerServiceCollection
{
    public static void AddSwagger(this IServiceCollection services, string title = "webApi doc", string securityKey = "token")
    {
        services.AddSwaggerGen(c =>
        {
            //foreach (FieldInfo fileld in typeof(ApiVersionInfo).GetFields())
            //{
            //    options.SwaggerDoc(fileld.Name, new OpenApiInfo
            //    {
            //        Version = fileld.Name,
            //        Title = "API标题",
            //        Description = $"API描述,{fileld.Name}版本"
            //    });
            //} 

            c.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });

            //c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "ZJHW_Common_System.WebApi.xml"));
            //c.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/App_Data/ApiXml/ZJHW_Common_System.WebApi.xml", true);// true表示显示控制器注释
            //c.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/App_Data/ApiXml/ZJHW_Common_System.Model.xml");
            //添加自定义配置 
            //c.OperationFilter<SwaggerConfig>();

            var xmls = Directory.GetFiles(Environment.CurrentDirectory, "M3.*.xml", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).ToList();
            xmls.ForEach(x => c.IncludeXmlComments(x, true));

            c.OrderActionsBy(q => q.RelativePath.Length.ToString());
            c.OrderActionsBy(api => api.HttpMethod);
            c.CustomSchemaIds(type => type.FullName);

            c.OperationFilter<SwaggerApiOperation>();
            c.SchemaFilter<AutoRestSchemaFilter>();
            c.AddSecurityDefinition("Auth", new OpenApiSecurityScheme
            {
                Description = "Token验证",
                Name = securityKey,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                //Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            var scheme = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "Auth" }
            };

            c.AddSecurityRequirement(new OpenApiSecurityRequirement() { { scheme, new string[] { "readAccess", "writeAccess" } } });

            //c.AddServer(new OpenApiServer()
            //{
            //    Url = "",
            //    Description = "服务地址一"
            //});
            //c.CustomOperationIds(apiDesc =>
            //{
            //    var controllerAction = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            //    return controllerAction.ControllerName + "-" + controllerAction.ActionName;
            //});
        });
    }
}


public class ApiVersionInfo
{
    public static string V1;
    public static string V2;
    public static string V3;
}