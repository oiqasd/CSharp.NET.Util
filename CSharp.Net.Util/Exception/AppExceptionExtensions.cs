namespace CSharp.Net.Util
{
    internal static class AppExceptionExtensions
    {
        /// <summary>
        /// 设置异常状态码
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static AppException ErrorCode(this AppException exception, int errorCode = 0)
        {
            exception.ErrorCode = errorCode;
            return exception;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static AppException WithData(this AppException exception, object data)
        {
            exception.Data = data;
            return exception;
        }
    }
}
