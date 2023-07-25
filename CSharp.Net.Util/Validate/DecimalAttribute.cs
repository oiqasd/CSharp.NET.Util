using System.Reflection;
using System.Text.RegularExpressions;

namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 正浮点型
    /// </summary>
    public class DecimalAttribute : BaseValidateAttribute
    {
        public int Num { get; set; }
        public int Point { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
        /// <summary>
        /// 验证是否是正浮点型
        /// </summary>
        /// <param name="num">整数位</param>
        /// <param name="point">小数点位</param>
        /// <param name="max">最大值</param>
        /// <param name="min">最小值</param>
        public DecimalAttribute(int num, int point, int max = 0, int min = 0)
        {
            this.Num = num;
            this.Point = point;
            this.Min = min;
            this.Max = max;
        }
        public override bool ValidateAction(object value, PropertyInfo property)
        {
            bool result = false;
            string message = "格式错误，保留" + Point + "位小数";
            string expression = @"^[0-9]\d{0," + (Num - Point - 1) + @"}(\.\d{0," + Point + @"})?$";
            Regex rx = new Regex(expression);
            if (rx.IsMatch(value.ToString()))
            {
                result = true;
                decimal v;
                if (!decimal.TryParse(value.ToString(), out v))
                    result = false;
                if (result)
                {
                    if (v <= 0)
                    {
                        result = false;
                        message = "必须大于0";
                    }
                    else
                    {
                        if (Max != 0 && Min != 0)
                        {
                            result = v < Max && v > Min;
                            if (!result) message = "数值应在" + Min + "至" + Max + "之间";
                        }
                        else if (Max != 0)
                        {
                            result = v < Max;
                            if (!result) message = "数值应小于" + Max;
                        }
                        else if (Min != 0)
                        {
                            result = v > Min;
                            if (!result) message = "数值大于" + Min;
                        }
                    }
                }
            }
            this.ErrorMessage = this.ErrorMessage + message;
            return result;
        }
    }
}

