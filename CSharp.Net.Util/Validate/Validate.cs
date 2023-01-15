using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util.Validate
{
    public class Validate
    {
        /// <summary>
        /// 封装统一数据验证方法
        /// </summary>
        /// <typeparam name="T">DTOModel</typeparam>
        /// <param name="t">需要验证的数据对象</param>
        /// <returns></returns>
        public static ValidateResult ValidataData<T>(T t) where T : class
        {
            ValidateResult result = new ValidateResult() { Flag = true };
            if (t == null)
            {
                result.Flag = false;
                result.ErrorMessage = "没有输入的数据";
                return result;
            } 
            foreach (var property in t.GetType().GetProperties())
            {
                if (property.IsDefined(typeof(BaseValidateAttribute), true))
                {
                    foreach (var attr in property.GetCustomAttributes(typeof(BaseValidateAttribute), true))
                    {
                        var value = property.GetValue(t);
                        if ((attr.GetType().Name != "RequiredAttribute") && (value == null || string.IsNullOrEmpty(value.ToString())))
                        {
                            continue;
                        }
                        var validate = attr as BaseValidateAttribute;
                        if (validate!=null)
                        {                            
                            var flag = validate.ValidateAction(value, property); // 执行验证
                            if (!flag)
                            {
                                // 验证不通过
                                result.Flag = false;
                                result.ErrorMessage = validate.ErrorMessage;
                                return result;                                
                            }
                        }
                    }
                }
            }
            return result;
        }


    }
}

