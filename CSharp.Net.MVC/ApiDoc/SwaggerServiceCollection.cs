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
        services.AddSwaggerGen(gen => InjectSwaggerGen(gen, securityKey));
    }

    static void InjectSwaggerGen(SwaggerGenOptions gen, string securityKey)
    {
        //foreach (FieldInfo fileld in typeof(ApiVersionInfo).GetFields())
        //{
        //    gen.SwaggerDoc(fileld.Name, new OpenApiInfo
        //    {
        //        Version = fileld.Name,
        //        Title = "API标题",
        //        Description = $"API描述,{fileld.Name}版本"
        //    });
        //}
        //gen.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
         
        var name = AppDomainHelper.AppName.Split('.')[0];
        var xmls = Directory.GetFiles(AppContext.BaseDirectory, $"{name}.*.xml",//Environment.CurrentDirectory
            new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).ToList();

        xmls.ForEach(x => gen.IncludeXmlComments(x, true));//true表示显示控制器注释

        gen.OrderActionsBy(q => q.RelativePath.Length.ToString());
        gen.OrderActionsBy(api => api.HttpMethod);
        gen.CustomSchemaIds(type => type.FullName);
        //添加自定义配置 
        gen.OperationFilter<SwaggerApiOperation>();
        gen.SchemaFilter<AutoRestSchemaFilter>();
        gen.AddSecurityDefinition("access-token", new OpenApiSecurityScheme
        {
            Description = "access-token",
            Name = securityKey,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            //Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        });

        var scheme = new OpenApiSecurityScheme()
        {
            Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "access-token" }
        };

        gen.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                { scheme, new string[] { "readAccess", "writeAccess" } }
        });

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
    }
}