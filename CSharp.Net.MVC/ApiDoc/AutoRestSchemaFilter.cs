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
