# CSharp.Net.Util

一个面向 .NET 项目的通用工具类库，提供日常开发中常用的扩展方法、辅助类和基础组件，减少重复代码，方便快速构建应用程序。

## 主要功能

- **日志**：支持 Debug、Info、Warn、Error 等日志级别，可输出到文件。
- **加密与安全**：提供 AES、DES、RSA、MD5、Hash、HMAC 等常用加密和摘要算法工具。
- **JSON 处理**：提供 JSON 序列化、反序列化、命名策略及常用类型转换器。
- **HTTP 与网络通信**：提供 HttpClient 请求辅助、请求扩展、Socket、TCP 和 WebSocket 基础组件。
- **文件与压缩**：提供文件操作、ZIP、GZip、Deflate 及其他压缩处理工具。
- **对象与数据转换**：提供实体转换、反射、对象属性、DataReader、DataRow 和集合转换扩展。
- **字符串与日期时间**：提供字符串处理、比较、正则、日期时间格式化及时间计算辅助方法。
- **集合与 LINQ 扩展**：提供 IEnumerable、IQueryable、Dictionary、数组、排序及表达式相关扩展。
- **分页与查询**：提供分页参数、分页结果和常用查询辅助模型。
- **验证**：提供必填、长度、数字、邮箱、手机号、身份证号等数据验证特性。
- **限流与重试**：提供令牌桶、限流器、重试及高精度延时等基础组件。
- **标识与并发工具**：提供雪花算法 ID、单例、延迟初始化字典、锁、共享内存和线程辅助工具。
- **业务辅助**：提供银行卡信息、身份证、IP、钉钉机器人、MSMQ 等常用辅助功能。
- **诊断与系统工具**：提供进程监控、应用域、内存、堆栈及部分底层系统操作辅助。

## 安装

```bash
dotnet add package CSharp.Net.Util
```

## 主要功能示例

以下示例展示常用入口，具体参数和返回值请以当前版本 API 为准。

### 日志

```csharp
LogHelper.Debug("应用程序启动成功");
LogHelper.Info("用户登录成功");
LogHelper.Error("处理请求失败");
```

### 加密与摘要

```csharp
using CSharp.Net.Util.Cryptography;

var md5 = MD5.Md5Encrypt("hello");
var base64 = MD5.MD5Base64String("hello");
```

### JSON

```csharp
var json = JsonHelper.Serialize(new { Name = "Tom", Age = 18 });
var user = JsonHelper.Deserialize<User>(json);

public class User
{
	public string Name { get; set; }
	public int Age { get; set; }
}
```

### 日期时间

```csharp
var date = DateTimeHelper.GetNowDate();
var time = DateTimeHelper.GeNowTime();
var dateTime = DateTimeHelper.GetNowDateTime();
```

### GZip 压缩

```csharp
using CSharp.Net.Util.Compression;

var compressed = GZipHelper.CompressString("需要压缩的文本");
var original = GZipHelper.DecompressString(compressed);
```

### 分页

```csharp
var page = new PageArgument(pageIndex: 2, size: 20);
var offset = page.ItemIndex;
```

`PageIndex` 从 1 开始，`PageSize` 未设置或小于等于 0 时默认为 10。

### 数据验证

```csharp
using CSharp.Net.Util.Validate;

var result = Validate.ValidataData(model);
if (!result.Flag)
{
	Console.WriteLine(result.ErrorMessage);
}

// 验证失败时抛出 ArgumentException
Validate.Vaild(model);
```

### 文件

```csharp
using CSharp.Net.Util;

var fileExists = FileHelper.IsFileExists("data.txt");
var directory = FileHelper.GetRealPath("data", "logs");
```

### HTTP 请求

`HttpClientUtil` 支持 GET、JSON POST、表单提交、文件上传、自定义请求头、超时和失败重试等常用场景。

```csharp

// GET
var content = await HttpClientUtil.GetAsync(
	"https://example.com/api/users",
	timeoutSeconds: 10,
	retryNum: 2);

// 带查询参数和请求头的 GET
var parameters = new Dictionary<string, string>
{
	["keyword"] = "test",
	["page"] = "1"
};
var headers = new Dictionary<string, string>
{
	["Authorization"] = "Bearer token"
};
var result = await HttpClientUtil.GetAsync(
	"https://example.com/api/search",
	parameters,
	headers,
	timeOutSecond: 10);

// JSON POST
var response = await HttpClientUtil.PostAsync(
	"https://example.com/api/users",
	new { Name = "Tom", Age = 18 },
	headers,
	timeOutSecond: 10);
```

默认请求失败时会抛出异常；如需失败时返回空字符串，可设置 `throwEx: false`：

```csharp
var content = await HttpClientUtil.GetAsync(
	"https://example.com/api/status",
	timeoutSeconds: 5,
	throwEx: false);
```

## 许可证

本项目采用 MIT 许可证。
