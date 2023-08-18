using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CSharp.Net.Mvc;

/// <summary>
/// api层帮助类
/// </summary>
public class ApiRefHelper
{
    /// <summary>
    /// 获取接口列表
    /// </summary>
    public void GetApiListByRefClassAttr()
    {
        List<ApiInfo> apis = new List<ApiInfo>();
        var assembly = Assembly.Load(this.GetType().Assembly.GetName().Name);
        //使用特性过滤
        var asses = assembly.GetTypes().Where(x => x.GetCustomAttributes().Where(v => v.GetType() == typeof(RefClassAttribute)).Any());
        foreach (var type in asses)
        {
            //if (!type.IsPublic)   continue;

            //if (type.BaseType.Name != nameof(BaseController) || type.Name == nameof(DictController))  continue;
           
            ApiInfo controller = new ApiInfo();
            controller.ControllerCode = type.Name;
            controller.ControllerDescribe = ((RefClassAttribute)type.GetCustomAttributes(typeof(RefClassAttribute), true).FirstOrDefault()).Name;
            controller.Sort = ((RefClassAttribute)type.GetCustomAttributes(typeof(RefClassAttribute), true).FirstOrDefault()).Sort;
            controller.ActionList = new List<ActionInfo>();
            foreach (var action in type.GetMethods().Where(x => x.Module.Name.StartsWith(this.GetType().Assembly.GetName().Name)))
            {
                var des = ((DescriptionAttribute)action.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault())?.Description;
                if (string.IsNullOrEmpty(des))
                    continue;

                controller.ActionList.Add(new ActionInfo
                {
                    ActionCode = action.Name,
                    ActionDescribe = des
                });
            }

            apis.Add(controller);

        }
    }
}
