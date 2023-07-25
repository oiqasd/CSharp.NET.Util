namespace CSharp.Net.Util.Validate
{
    /// <summary>
    /// 通用返回验证结果对象
    /// </summary>
    public class ValidateResult
    {
        /// <summary>
        /// 验证是否通过,true：通过，false：不通过
        /// </summary>
        public bool Flag { get; set; }

        /// <summary>
        /// 错误提示
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

