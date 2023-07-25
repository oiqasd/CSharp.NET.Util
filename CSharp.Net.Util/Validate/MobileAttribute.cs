using System.Reflection;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 验证手机号格式
    /// </summary>
    public class MobileAttribute : BaseValidateAttribute
    {
        public override bool ValidateAction(object value, PropertyInfo property)
        {
            if (value == null)
            {
                this.ErrorMessage = $"手机号码不能为空";
                return false;
            }
            bool v = RegexHelper.CheckPhone(value.ToString());
            if (v)
                return true;

            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ErrorMessage = $"手机格式不正确";
            }
            return false;
        }
    }
}

