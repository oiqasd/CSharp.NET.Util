using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public class ReflectionHelper
    {
        /// <summary>
        /// 获取调用链方法名
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string GetCallingMethodName(int depth = 1)
        {
            StackTrace stack = new StackTrace();
            return stack.GetFrame(depth)?.GetMethod()?.Name;  
            //获取当前
            //System.Reflection.MethodBase.GetCurrentMethod().Name;
        }

        /// <summary>
        /// 获取调用链类名
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string GetCallingClassName(int depth = 1)
        {
            StackTrace stack = new StackTrace();
            return stack.GetFrame(depth)?.GetMethod()?.DeclaringType?.ToString();
            //System.Reflection.Assembly.GetCallingAssembly();
            //获取当前
            //this.GetType().Name
        }


        /// <summary>
        /// 判断值类型或者无参构造函数
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool HasDefaultConstructor(Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// 获取私有构造函数
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ConstructorInfo[] GetConstructorInfo(Type t)
        {
            var r = t.GetConstructors().OrderBy(x => x.GetParameters().Length - x.GetParameters().Count(p => p.IsOptional)).ToArray();
            return r;
            //return t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        }


    }
}
