using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CSharp.Net.Mvc
{
    /// <summary>
    /// 自定义接口注入
    /// </summary>
    public static class ControllerActivator
    {
        public static IMvcBuilder AddControllerServices(this IMvcBuilder builder)
        {
            //获取MVC应用程序中控制器类型的列表
            var feature = new ControllerFeature();
            builder.PartManager.PopulateFeature(feature);

            foreach (var controller in feature.Controllers.Select(x => x.AsType()))
            {
                builder.Services.TryAddTransient(controller, controller);
            }
            builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, AppControllerActivator>());//ServiceBasedControllerActivator
            return builder;
        }
    }

    public class AppControllerActivator : IControllerActivator
    {
        public object Create(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var ctype = context.ActionDescriptor.ControllerTypeInfo.AsType();
            var controller = context.HttpContext.RequestServices.GetRequiredService(ctype);
            //自定义属性注入
            if (controller is BaseController bc)
            {
                bc.Logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(ctype);
            }
            return controller;
        }

        public void Release(ControllerContext context, object controller)
        {
           
        }
    }
}
