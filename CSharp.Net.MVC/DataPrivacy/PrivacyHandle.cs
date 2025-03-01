// ****************************************************
// * 创建日期：2023-7-17
// * 创建人：
// * 备注：
// ****************************************************

using CSharp.Net.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CSharp.Net.Mvc;

/// <summary>
/// 签名验证 <see cref="appsetting.json"/>
/// <code> "ApiSign": {
///      "IgnoreMethod": "GET",         //忽略验证请求，多个用,隔开
///      "Algorithm": "MD5",            //加密算法，md5,rsa,aes等，默认md5
///      "AppKey": "",                  //密钥
///      "SignField": "sign",           //自定义签名字段，默认sign
///      "CheckExpired": {              //时间戳校验请求过期
///         "ExpiredField":"timestamp", //字段名，默认timestamp
///         "ExpiredSeconds": 1800      //秒,过期时间(请求时毫秒、秒皆可)
///        }       
///   }</code>
/// </summary>
public class PrivacyHandle : ActionFilterAttribute
{
    IConfiguration _configuration { get; }
    public PrivacyHandle(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attrList = context.ActionDescriptor.EndpointMetadata.Select(e => e.GetType().Name).ToList();
        var _focre = attrList.Contains(nameof(ForcePrivacyAttribute));
        if (!_focre && (attrList.Contains(nameof(AllowAnonymousAttribute)) || attrList.Contains(nameof(IngorePrivacyAttribute))))
        {
            await next();
            return;
        }

        var parmObj = context.ActionArguments.FirstOrDefault().Value;
        var parmObjProp = parmObj?.GetType().GetProperties();
        //var result = Validate.ValidataData(parmObj);
        //if (!result.Flag)  throw new SignErrorException(result.ErrorMessage);
        var requestData = await context.HttpContext.Request.ReadDataAsync().ConfigureAwait(false);

        //先验证过期
        int expiredSeconds = ConvertHelper.ConvertTo(_configuration["ApiSign:CheckExpired:ExpiredSeconds"], 0);
        if (expiredSeconds > 0)
        {
            var expiredField = _configuration["ApiSign:CheckExpired:ExpiredField"] ?? "timestamp";
            long times = requestData.GetValueM(expiredField, 0L);
            if (times <= 0)
                times = ConvertHelper.ConvertTo(parmObjProp.FirstOrDefault(q => q.Name.ToLower() == expiredField)?.GetValue(parmObj), 0L);
            var clientTime = DateTimeHelper.GetDateTimeFromTimeStamp(times);
            if (Math.Abs(DateTimeHelper.DateDiff(DateInterval.Second, clientTime, DateTime.UtcNow)) > expiredSeconds)
                throw new RequestExpiredException();
        }

        var ignoreMethod = _configuration["ApiSign:IgnoreMethod"];
        if (!_focre && ignoreMethod.IsNotNullOrEmpty())
        {
            var _m = ignoreMethod.Split(',');
            if (_m.Contains(context.HttpContext.Request.Method, StringComparer.OrdinalIgnoreCase))
            {
                await next();
                return;
            }
        }
        string signField = _configuration["ApiSign:SignField"] ?? "sign";
        var signData = requestData.GetValue(signField)?.ToString();
        if (signData == null)
            signData = parmObjProp.FirstOrDefault(q => q.Name.ToLower() == signField)?.GetValue(parmObj) as string;

        if (signData.IsNullOrEmpty()) throw new SignNotFoundException();

        /*
        SortedList<string, string> listd = new SortedList<string, string>();
        PropertyInfo parmPro = parmObjProp.FirstOrDefault(q => q.Name.ToLower() == "parameter");
        if (parmPro != null)
        {
            var pv = parmPro.GetValue(parmObj);
            var fields = pv?.GetType().GetProperties().Where(q => q.GetCustomAttributes<SignFieldAttribute>().Any()).ToList() ?? new List<PropertyInfo>();
            foreach (var s in fields)
            {
                listd.Add(s.Name, s.GetValue(pv)?.ToString());
            }
        }*/

        var listdata = GetSignFields(parmObj);
        var listdstr = string.Join("", listdata.Select(q => q.Value));
        var cryp = ApiSign.Cryp.CrypFactory.SetCryp(_configuration["ApiSign:Algorithm"]);
        var sign = cryp.Encrypt(listdstr, _configuration["ApiSign:AppKey"]);

        if (!string.Equals(signData, sign, StringComparison.OrdinalIgnoreCase))
        {
            throw new SignErrorException("签名错误");
        }
        await next();
    }


    SortedList<string, string> GetSignFields(object parmObj, int deep = 0, string parentName = "")
    {
        SortedList<string, string> list = new SortedList<string, string>();
        var parmObjProp = parmObj?.GetType().GetProperties();
        if (parmObjProp.IsNullOrEmpty()) return list;

        foreach (var col in parmObjProp)
        {
            if (col.PropertyType.IsEnum)//注意:可空字段Enum不会进入
            {
                if (!col.GetCustomAttributes<SignFieldAttribute>().Any()) continue;
                //获取枚举成员的值
                var val = col.GetValue(parmObj);
                if (val == null) continue;
                list.Add($"{deep}.{parentName}.{col.Name}", ConvertHelper.ConvertTo(val, 0).ToString());
            }
            else if (col.PropertyType.IsValueType || col.PropertyType.IsPrimitive || col.PropertyType.Name == nameof(String))
            {
                if (col.GetCustomAttributes<SignFieldAttribute>().Any())
                    list.Add($"{deep}.{parentName}.{col.Name}", col.GetValue(parmObj)?.ToString());
            }
            else
            {
                deep++;
                var child = GetSignFields(col.GetValue(parmObj), deep, col.Name);
                child.ForEach(x => list.Add(x.Key, x.Value));
            }
        }
        return list;
    }

}
