using CSharp.Net.Util;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CSharp.Net.Mvc;

internal class SwaggerGenOptionsExt : SwaggerGenOptions
{
    public string ApiName { get; set; }
}

internal class SwaggerConfigureOptions : IConfigureOptions<SwaggerGenOptions>
{
    readonly IApiDescriptionGroupCollectionProvider provider;
    //readonly IApiVersionDescriptionProvider provider;

    public SwaggerConfigureOptions(IApiDescriptionGroupCollectionProvider provider) => this.provider = provider;

    public void Configure(string name, SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiDescriptionGroups.Items)
        {
            options.SwaggerDoc(description.GroupName ?? "default", CreateInfoForApiDesc(name, description));
        }
    }

    public void Configure(SwaggerGenOptions options) => Configure(Options.DefaultName, options);

    static OpenApiInfo CreateInfoForApiDesc(string name, ApiDescriptionGroup description)
    {
        var info = new OpenApiInfo()
        {
            Title = name.IsNotNullOrEmpty() ? name : AppDomainHelper.AppName ?? "default",
            Version = description.GroupName
        };
        return info;
    }

    //static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    //{
    //    var info = new OpenApiInfo()
    //    {
    //        Title = Assembly.GetEntryAssembly()?.GetName().Name,
    //        Version = "v" + description.ApiVersion.ToString(),
    //    };
    //    if (description.IsDeprecated)
    //        info.Description += "此 Api " + info.Version + " 版本已弃用，请尽快升级新版";
    //    return info;
    //}
}
