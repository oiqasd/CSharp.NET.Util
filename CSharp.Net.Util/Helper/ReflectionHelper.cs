using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSharp.Net.Standard.Util
{
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
            return stack.GetFrame(depth).GetMethod().Name;


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
            return stack.GetFrame(depth).GetMethod().DeclaringType.ToString();

            //获取当前
            //this.GetType().Name
        }
         
    }
}
