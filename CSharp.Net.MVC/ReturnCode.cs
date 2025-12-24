using System.ComponentModel;

namespace CSharp.Net.Mvc
{
    public enum ReturnCode
    {
        OK = 0,

        #region http code
        /// <summary>
        /// Get成功，无错误
        /// </summary>
        [Description("OK")]
        HttpOk = 200,
        /// <summary>
        /// [POST/PUT/PATCH]：用户新建或修改数据成功
        /// 暂时统一都用200
        /// </summary>
        CREATED = 201,
        /// <summary>
        /// 表示一个请求已经进入后台排队（异步任务）
        /// </summary>
        [Description("请求成功，等待执行")]
        Accepted = 202,

        /// <summary>
        /// 请求成功，无数据返回
        /// </summary>
        [Description("请求成功，无数据返回")]
        NoContent = 204,

        /// <summary>
        /// 协议或者参数非法,[POST/PUT/PATCH]：用户发出的请求有错误，服务器没有进行新建或修改数据的操作，该操作是幂等的。
        /// </summary>
        [Description("协议或者参数错误")]
        PamramerError = 400,

        /// <summary>
        /// 表示用户没有权限（令牌、用户名、密码错误）,未登录,签名验证失败SIGN_ERROR
        /// </summary>
        [Description("未经授权，请先登录")]
        UnAuthorized = 401,

        /// <summary>
        /// 资源需要付款
        /// </summary>
        [Description("需要付款")]
        PaymentRequired = 402,

        /// <summary>
        /// 表示用户得到授权（与401错误相对），但是访问是被禁止的。
        /// </summary>
        [Description("用户得到授权，但是无权访问")]
        Forbidden = 403,

        /// <summary>
        /// 数据不存在
        /// 用户发出的请求针对的是不存在的记录，服务器没有进行操作，该操作是幂等的
        /// </summary>
        [Description("数据不存在")]
        DataNotFound = 404,

        /// <summary>
        /// 请求超时
        /// </summary>
        [Description("请求超时")]
        RequestTimeOut = 408,

        /// <summary>
        /// 不支持的媒体类型/Unsupported Media Type
        /// 检查一下头部的Accept字段和Content-type字段
        /// </summary>
        [Description("不支持的媒体类型")]
        UnsupportedMediaType = 415,

        /// <summary>
        /// 请求超过频率限制
        /// </summary>
        [Description("请求超过频率限制")]
        TooManyRequests = 429,

        /// <summary>
        /// 系统错误
        /// </summary>
        [Description("系统错误")]
        SystemError = 500,

        /// <summary>
        /// 接口未实现
        /// </summary>
        [Description("接口未实现")]
        NotImplemented = 501,

        /// <summary>
        /// 服务下线，暂时不可用
        /// </summary>
        [Description("服务下线，暂时不可用")]
        BadGateway = 502,

        /// <summary>
        /// 服务不可用，过载保护
        /// </summary>
        [Description("服务不可用，过载保护")]
        ServiceUnavailable = 503,

        /// <summary>
        /// 上游服务器超时
        /// </summary>
        [Description("网关超时")]
        GatewayTimeout = 504,
        #endregion

        #region 1000-通用类
        [Description("header获取host失败")]
        HeaderNoHost = 1000,

        /// <summary>
        /// 商户不存在
        /// </summary>
        [Description("商户不存在")]
        MerchantNotExist = 1001,

        /// <summary>
        /// 数据已存在
        /// </summary>
        [Description("数据已存在")]
        DataExisted = 1002,

        /// <summary>
        /// 数据错误
        /// </summary>
        [Description("数据错误")]
        DataError = 1004,

        [Description("配置错误")]
        ConfigError = 1005,

        [Description("重复提交")]
        RepeatCommit = 1006,

        [Description("过期提交")]
        ExpiredRequest = 1007,

        [Description("签名错误")]
        CheckSignError = 1008,
        #endregion

        #region 2000-用户类

        [Description("登录失败")]
        LoginFail = 2000,

        [Description("账号或密码错误")]
        UserAccountPassordError = 2001,

        [Description("您的账号已被锁定，请联系管理员")]
        UserAccountLocked = 2002,

        [Description("用户不存在")]
        UserNotExist = 2003,

        [Description("用户已存在")]
        UserExisted = 2004,

        #endregion

        [Description("短信发送失败")]
        SmsSendError = 3001,

        [Description("未知错误")]
        Unknown = 999999,
    }
}
