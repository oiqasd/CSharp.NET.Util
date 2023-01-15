using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharp.Net.Mvc
{
    public static class RequestHelper
    {
        #region
        /// <summary>
        /// 响应用户请求 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method">"POST" or "GET"</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static StringBuilder GetResponse(string url, string method, string data)
        {
            StringBuilder builder = new StringBuilder(256);
            switch (method)
            {
                case "post":
                    builder.AppendFormat("<html><body><form id='submitdata' method='post' action='{0}'>", url);
                    string[] arg1 = data.Split('&');
                    for (int i = 0; i < arg1.Length; i++)
                    {
                        string[] arg2 = arg1[i].Split('=');
                        builder.AppendFormat("<input type='hidden' name='{0}' value={1} id='{2}'><br/>", arg2[0], arg2[1], arg2[0]);
                    }
                    builder.Append("</form><script language='javascript'> document.getElementById('submitdata').submit();</script></body></html>");
                    return builder;
                case "get":
                    builder.AppendFormat("{0}?", url);
                    builder.Append(data);
                    return builder;
            }
            return builder;
        }
        #endregion

        #region 获取用户Form提交的字段值
        /// <summary>
        /// 获取post和get提交值
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="inputName">字段名</param>
        /// <param name="method">post和get</param>
        /// <returns></returns>
        public static T Get<T>(string inputName, MethodType method)
        {
            string tempValue = GetPostOrRequestValue(inputName, method);
            T result = ChangeTypeValue<T>(tempValue);
            return result;
        }

        /// <summary>
        /// 转换类型的值
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="inputValue">输入的字符串</param>
        /// <returns></returns>
        private static T ChangeTypeValue<T>(string inputValue)
        {
            T result = default(T);
            Type resultType = typeof(T);
            if (resultType.IsValueType)
            {
                MethodInfo tryParse = null;
                if (resultType.IsEnum)
                {
                    MethodInfo[] methodList = typeof(Enum).GetMethods();
                    if (methodList != null)
                    {
                        IEnumerator ide = methodList.GetEnumerator();
                        while (ide.MoveNext())
                        {
                            MethodInfo tmpInfo = (MethodInfo)ide.Current;
                            if (tmpInfo.Name.Equals("TryParse") && tmpInfo.IsGenericMethod && tmpInfo.GetParameters().Length == 2)
                            {
                                tryParse = tmpInfo;
                                break;
                            }
                        }
                    }
                    tryParse = tryParse.MakeGenericMethod(typeof(T));
                }
                else
                {
                    tryParse = resultType.GetMethod("TryParse", new Type[] { typeof(String), typeof(T).MakeByRefType() });
                }
                object[] paramArray = new object[] { inputValue, result };
                if (tryParse != null)
                {
                    bool flag = (bool)tryParse.Invoke(null, paramArray);
                    if (flag)
                        result = (T)paramArray[1];
                }
            }
            else
            {
                result = (T)Convert.ChangeType(inputValue, typeof(T));
            }
            return result;
        }

        /// <summary>
        /// 获取post或get的值
        /// </summary>
        /// <param name="inputName">参数</param>
        /// <param name="method">提交类型</param>
        /// <returns></returns>
        public static string GetPostOrRequestValue(string inputName, MethodType method)
        {
            var context = HttpContextExt.Current;
            string tempValue = string.Empty;
            #region 获取提交字段数据 TempValue

            if (method == MethodType.Post)
            {
                if (context.Request.HasFormContentType && context.Request.Form.ContainsKey(inputName))
                {
                    tempValue = context.Request.Form[inputName].FirstOrDefault();
                }
            }
            else if (method == MethodType.Get)
            {
                if (context.Request.Query.ContainsKey(inputName))
                {
                    tempValue = context.Request.Query[inputName].FirstOrDefault();
                }
            }
            else if (method == MethodType.All)
            {
                if (context.Request.Query.ContainsKey(inputName))
                {
                    tempValue = context.Request.Query[inputName].ToString();
                }

                if (String.IsNullOrEmpty(tempValue))
                {
                    if (context.Request.HasFormContentType && context.Request.Form.ContainsKey(inputName))
                    {
                        tempValue = context.Request.Form[inputName].ToString();
                    }
                }
            }
            #endregion
            return tempValue;
        }

        #endregion

        #region 正式表达式验证 Help Functions

        /// <summary>
        /// 正式表达式验证
        /// </summary>
        /// <param name="C_Value">验证字符</param>
        /// <param name="C_Str">正式表达式</param>
        /// <returns>符合true不符合false</returns>
        private static bool CheckRegEx(string C_Value, string C_Str)
        {
            Regex objAlphaPatt;
            objAlphaPatt = new Regex(C_Str, RegexOptions.Compiled);

            return objAlphaPatt.Match(C_Value).Success;
        }

        #endregion
    }
    /// <summary>
    /// 获取请求数据方式
    /// </summary>
    public enum MethodType
    {
        All = 0,

        /// <summary>
        /// Post方式
        /// </summary>
        Post = 1,

        /// <summary>
        /// Get方式
        /// </summary>
        Get = 2

    }

}
