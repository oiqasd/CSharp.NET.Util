namespace CSharp.Net.Util
{
    internal static class AppExceptionExtensions
    {
        /// <summary>
        /// 设置异常状态码
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static AppException StatusCode(this AppException exception, int statusCode = 0)
        {
            exception.StatusCode = statusCode;
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
