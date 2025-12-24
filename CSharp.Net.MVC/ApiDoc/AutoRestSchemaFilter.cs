// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Mvc;

public class AutoRestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        // 解决泛型 Response<T> 在Swagger 中无法正确显示T类型的问题
        // 检查当前处理的类型是否是 Response<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Response<>))
        {
            // 获取泛型参数 T 的类型（例如 List<AdminUser>）
            Type genericArgumentType = context.Type.GetGenericArguments()[0];
            // 为 T 类型生成 Schema
            var dataSchema = context.SchemaGenerator.GenerateSchema(genericArgumentType, context.SchemaRepository);

            // 找到 schema 中名为 "Data" 的属性，并替换它的 Schema
            // 假设你的 Response<T> 类中有一个名为 Data 的属性
            if (schema.Properties.TryGetValue("data", out var dataPropertySchema))
            {
                schema.Properties["data"] = dataSchema;
            }
        }

        if (type.IsEnum)
        {
            //schema.Extensions.Add(
            //    "x-ms-enum",
            //    new OpenApiObject
            //    {
            //        ["name"] = new OpenApiString(type.Name),
            //        ["modelAsString"] = new OpenApiBoolean(true)
            //    }
            //);

            schema.Enum.Clear();
            var str = new StringBuilder($"{schema.Description}(");
            foreach (var value in Enum.GetValues(type))
            {
                var fieldInfo = type.GetField(Enum.GetName(type, value));
                var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>(true);
                schema.Enum.Add(OpenApiAnyFactory.CreateFromJson(JsonHelper.Serialize(value)));

                str.Append($"{(int)value}:{descriptionAttribute?.Description},");
            }

            str.Append(')');
            schema.Description = str.ToString();
        };
    }
}
