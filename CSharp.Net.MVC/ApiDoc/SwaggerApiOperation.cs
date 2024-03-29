﻿// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace CSharp.Net.Mvc;

public class SwaggerApiOperation : IOperationFilter
{
    /// <summary>
    /// 初始化模块配置
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation == null || context == null)
            return;
        operation.Parameters = operation.Parameters ?? new List<OpenApiParameter>();
        //var isAuthor = context.ApiDescription.ControllerAttributes().Any(e => e.GetType() == typeof(AllowAnonymousAttribute)) || context.ApiDescription.ActionAttributes().Any(e => e.GetType() == typeof(AllowAnonymousAttribute));//2.2
        //var isAuthor = context.MethodInfo.CustomAttributes.Any(q => q.AttributeType.Name == "AllowAnonymousAttribute");  //3.0      
        //if (!isAuthor)
        //{
        //    operation.Parameters.Insert(0, new NonBodyParameter() { Name = "Authorization", In = "header", Description = "身份验证Token", Required = true, Type = "string" });
        //}
        //if (!operation.Parameters.Any(x => x.Name.ToLower() == "issn"))
        //    operation.Parameters.Insert(0, new OpenApiParameter() { Name = "Issn", In = ParameterLocation.Header, Description = "ISSN", Required = false, AllowEmptyValue = false });

        var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
        .Union(context.MethodInfo.GetCustomAttributes(true))
        .OfType<AllowAnonymousAttribute>();
        //.OfType<AuthorizeAttribute>();

        if (!authAttributes.Any())
        {
            //in query header  
            operation.Parameters.Insert(0, new OpenApiParameter()
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "身份验证Token",
                Required = true,
                AllowEmptyValue = false
            });
        }
        return;
        /**
        var files = context.ApiDescription.ActionDescriptor.Parameters
                        .Where(n => n.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
                        .OfType<SwaggerFileUploadAttribute>()
                        .ToList();
        if (files.Count > 0)
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (i == 0)
                {
                    operation.Parameters.Clear();
                }
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = files[i].Name,
                    In = ParameterLocation.Header,
                    Description = "上传文件",
                    Required = true,
                    //Type = "file"
                });
            }

#if NETCOREAPP3_0_OR_GREATER
            operation.RequestBody.Content.Add("multipart/form-data", new OpenApiMediaType());
#else
            operation.Consumes.Add("multipart/form-data");//2.2
#endif
        */
    }
}
