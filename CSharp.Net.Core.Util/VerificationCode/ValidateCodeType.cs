/// <summary>
/// 验证码类型
/// </summary>

namespace CSharp.Net.Core.Util
{
    public enum ValidateCodeType
    {
        /// <summary>
        /// 纯数值
        /// </summary>
        Number,

        /// <summary>
        /// 数值与字母的组合
        /// </summary>
        NumberAndLetter,

        /// <summary>
        /// 汉字
        /// </summary>
        Hanzi
    }
}
