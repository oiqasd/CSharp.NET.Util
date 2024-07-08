namespace CSharp.Net.Mvc
{
    public class Response
    {
        public Response(ReturnCode code)
        {
            this.Code = code;
            this.Message = code.GetDescription();
        }
        public Response(ReturnCode code, string msg)
        {
            this.Code = code;
            this.Message = msg;
        }
        /// <summary>
        /// 状态
        /// </summary>
        public object Code { get; private set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; private set; }
    }


    /// <summary>
    ///  api 响应对象
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class Response<TData> : Response
    {
        /// <summary>
        /// 正常返回
        /// </summary>
        /// <param name="data"></param>
        public Response(TData data) : base(ReturnCode.OK)
        {
            this.Data = data;
        }

        ///// <summary>
        ///// 执行处理返回
        ///// </summary>
        ///// <param name="exceResult"></param>
        //public Response(ExceResult<TData> exceResult) : base(exceResult.Code, exceResult.Message)
        //{
        //    this.Data = exceResult.Data;
        //}

        /// <summary>
        /// 错误，无数据返回
        /// </summary>
        /// <param name="code"></param> 
        public Response(ReturnCode code) : base(code) { }

        /// <summary>
        /// 错误，带数据返回
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        public Response(ReturnCode code, TData data) : base(code)
        {
            this.Data = data;
        }
        /// <summary>
        /// 错误，带提示返回
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public Response(ReturnCode code, string msg) : base(code, msg) { }

        /// <summary>
        /// 错误，带提示返回
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public Response(ReturnCode code, string msg, TData data) : base(code, msg)
        {
            this.Data = data;
        }
        /// <summary>
        /// 返回数据
        /// </summary>
        public TData Data { get; private set; }
    }
}
