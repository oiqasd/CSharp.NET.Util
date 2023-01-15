using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 验证字段是否是正整数
    /// </summary>
    public class IntAttribute : BaseValidateAttribute
    {
        public override bool ValidateAction(object value, PropertyInfo property)
        {
            Regex rx = new Regex(@"^[1-9]\d*$");
            if (value == null || rx.IsMatch(value.ToString()))
            {
                return true;
            }
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ErrorMessage = $"字段格式需要是正整数";
            }
            return false;
        }
    }
}

