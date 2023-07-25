using System.Reflection;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 验证字段非空
    /// </summary>
    public class RequiredAttribute : BaseValidateAttribute
    {
        public override bool ValidateAction(object value,PropertyInfo property)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ErrorMessage = $"字段不能为空";
            }
            return false;
        }
    }
}

