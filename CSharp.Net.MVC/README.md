### 前言
对MVC框架进行比较完整的封装，对代码无入侵，开发可直接通过环境变量完成对本框架的激活，支持跨平台部署。
不再支持.NET8以前的版本

### 全局功能
1、集成`Configuration`配置，全局使用通过`App.Configuration`访问，全局 Httpontext`App.HttpContext`
2、默认使用`log4net日志`，并实现`Trace`日志监听及文件输出
3、跨域配置可通过`Cors:WithOrigins`
4、默认绑定全局异常处理器`ExceptionHandle`,异常日志输出通过`"Logger": {"IsOpening": true},`打开
5、集成Swagger文档，可加密访问，配置参考*组件配置*
6、默认配置健康检查接口`/health`
7、默认运行时接口`/runtime`
**8、接口加密配置**通过配置启动
   
- 通过`ForcePrivacy`特性配置接口强制启用加密
- 用`AllowAnonymous`和`IngorePrivacy`忽略加密，优先级低于`ForcePrivacy`
- 用`SignField`字段特性，配置加密规则的字段，支持多层级
- `Appsetting.cofing`添加配置
```
    "ApiSign": {
      "IgnoreMethod": "GET",         //忽略验证请求，多个用,隔开
      "Algorithm": "MD5",            //加密算法，md5,rsa,aes等，默认md5
      "AppKey": "",                  //密钥
      "SignField": "sign",           //自定义签名字段，默认sign
      "CheckExpired": {              //时间戳校验请求过期
         "ExpiredField":"timestamp", //字段名，默认timestamp
         "ExpiredSeconds": 1800      //秒,过期时间(请求时毫秒、秒皆可)
        }       
      }
```
9、如要自建DI可参考`AppControllerActivator`

- 组件全局配置,`Appsetting.cofing`添加如下
```
  "CMVC": {
    "ApiDoc": true,     //启动接口文档
    "VirPath": "",      //二级地址
    "AuthKey": "xxx",   //组件加密访问模块的key
    "AuthValue": "xxx"  //组件加密访问模块的value
  },
```

### 接口功能
- 可继承`BaseController`，将获得：
1、默认路由配置`api/[controller]/[action]`
2、默认跨域
3、默认接口文档分组`default`
4、默认接口异常处理器`ApiException`，自动区分业务异常和系统异常，统一的包装返回，异常日志记录
5、默认的统一返回`Response`的格式, 使用 `return Success()`或 `Fail()`
```
    {
        "code":0,
        "message":"",
        "data": T
    }
```

### 已有但未纳入的模块
- 缓存组件，需项目单独引入`CSharp.Net.Cache`，默认可选支持`redis、memorycache`
- 接口用户用户登录状态管理
- 并发请求处理
- 接口请求参数校验和返回参数的初始化处理
- 接口性能记录

### 其它常用帮助类

- 反射所有接口列表 `ApiRefHelper.GetApiListByRefClassAttr`,需配置反射特性`RefClass`
- `App.Configuration`，获取配置
- `App.RootServices`，获取根容器
- `App.HttpContext`，获取HttpContext
- `App.GetService`，解析服务
- `App.GetThreadId`，当前线程ID
- `HttpContext`扩展
  获取客户端IP:`.GetRemoteIP()`
  获取本机IP:`.GetLocalIP()`
  获取完整请求地址:`.GetRequestUrl()`
  获取完整请求地址:`.GetRequestUrl()`
  获取Action特性:`.GetMetadata()`
  获取控制器/Action特性:`.GetControllerActionDescriptor()`
  写Cookie:`.SetCookie()`
- 另`MvcHelper`