using System.Reflection;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 验证字符串长度
    /// </summary>
    public class LengthAttribute : BaseValidateAttribute
    {
        public int Min { get; set; }
        public int Max { get; set; }        

        public LengthAttribute(int min,int max)
        {
            this.Min = min;
            this.Max = max;           
        }

        public override bool ValidateAction(object value, PropertyInfo property) 
        {
            var strVal = value.ToString();
            if (strVal.Length >= this.Min && strVal.Length <= this.Max)
            {
                return true;
            }
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ErrorMessage = $"字段长度应该在{this.Min}和{this.Max}之间";
            }
            return false;           
        }
    }
}

