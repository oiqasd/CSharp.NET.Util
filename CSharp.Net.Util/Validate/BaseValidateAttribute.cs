using System;
using System.Reflection;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 数据验证特性基类
    /// </summary>
    public abstract class BaseValidateAttribute : Attribute
    {
        /// <summary>
        /// 错误提示
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 执行验证
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="property">对应的属性对象</param>
        /// <returns></returns>
        public abstract bool ValidateAction(object value, PropertyInfo property);

    }
}

