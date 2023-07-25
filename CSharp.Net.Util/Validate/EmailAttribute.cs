using System.Reflection;
using System.Text.RegularExpressions;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 验证Email格式
    /// </summary>
    public class EmailAttribute : BaseValidateAttribute
    {
        public override bool ValidateAction(object value, PropertyInfo property)
        {
            Regex rx = new Regex(@"^[A-Za-z0-9\u4e00-\u9fa5]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$");
            if (rx.IsMatch(value.ToString()))
            {
                return true;
            }
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ErrorMessage = $"邮箱格式不正确";
            }
            return false;
        }
    }
}

