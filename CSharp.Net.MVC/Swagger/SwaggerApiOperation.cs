// ****************************************************
// * 创建日期：
// * 创建人：
// * 备注：
// ****************************************************

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.AspNetCore.Swagger
{
    public class SwaggerApiOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null) return;

            operation.Parameters = operation.Parameters ?? new List<OpenApiParameter>();

            if (!operation.Parameters.Any(x => x.Name.ToLower() == "issn"))
                operation.Parameters.Insert(0, new OpenApiParameter() { Name = "Issn", In = ParameterLocation.Header, Description = "ISSN", Required = false, AllowEmptyValue = false });
        }
    }

}
