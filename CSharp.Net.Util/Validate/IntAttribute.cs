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
        int Max { get; set; }
        int Min { get; set; }

        public IntAttribute(int max = int.MaxValue, int min = int.MinValue)
        {
            this.Min = min;
            this.Max = max;
        }

        public override bool ValidateAction(object value, PropertyInfo property)
        {
            Regex rx = new Regex(@"^[1-9]\d*$");

            if (value == null) return true;
            if (!rx.IsMatch(value.ToString()))
            {
                this.ErrorMessage = $"字段格式需要是正整数";
                return false;
            }
            var v = ConvertHelper.ConvertTo(value, 0);
            if (v > Max)
            {
                this.ErrorMessage = $"字段不能大于{Max}";
                return false;
            }
            if (v < Min)
            {
                this.ErrorMessage = $"字段不能小于{Min}";
                return false;
            }
            return true;
        }
    }
}

