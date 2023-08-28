using CSharp.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;
using System.Linq;

namespace CSharp.Net.Mvc;

public static class SwaggerServiceCollection
{
    public static void AddSwagger(this IServiceCollection services, string title = "webApi doc", string securityKey = "token")
    {
        //services.ConfigureAll<SwaggerGenOptionsExt>(x => x.ApiName = title);
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
        //services.AddTransient<IConfigureOptions<SwaggerGenOptions>>();
        services.AddSwaggerGen(c =>
        {
            //foreach (FieldInfo fileld in typeof(ApiVersionInfo).GetFields())
            //{
            //    c.SwaggerDoc(fileld.Name, new OpenApiInfo
            //    {
            //        Version = fileld.Name,
            //        Title = "API标题",
            //        Description = $"API描述,{fileld.Name}版本"
            //    });
            //}

            //c.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
            //c.IncludeXmlComments($@"{_hostingEnvironment.ContentRootPath}/App_Data/ApiXml/ZJHW_Common_System.WebApi.xml", true);// true表示显示控制器注释

            var name = AppDomainHelper.AppName.Split('.')[0];
            var xmls = Directory.GetFiles(AppContext.BaseDirectory, $"{name}.*.xml",//Environment.CurrentDirectory
                new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).ToList();

            xmls.ForEach(x => c.IncludeXmlComments(x, true));

            c.OrderActionsBy(q => q.RelativePath.Length.ToString());
            c.OrderActionsBy(api => api.HttpMethod);
            c.CustomSchemaIds(type => type.FullName);
            //添加自定义配置 
            c.OperationFilter<SwaggerConfig>();

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
    public static string Default;
    public static string V1;
    public static string V2;
    public static string V3;
}