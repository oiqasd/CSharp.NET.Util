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
            //Assembly.GetExecutingAssembly().GetName().Name
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

        /// <summary>
        /// 反射获取所有带有<para>TAttribute</para>特性的类
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assemblyName">指定程序集,默认全部</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetClassesByAttribute<TAttribute>(params string[] assemblyName) where TAttribute : Attribute
        {
            IEnumerable<Type> classes = null;
            foreach (var name in assemblyName)
            {
                var asses = Assembly.Load(name).GetTypes().Where(type => type.GetCustomAttributes<TAttribute>().Any());
                if (asses.IsNullOrEmpty()) continue;
                classes = classes.IsNullOrEmpty() ? asses : asses.Union(asses);
            }
            if (assemblyName.IsNullOrEmpty())
            {
                foreach (var t in AppDomainHelper.GetAssemblies())
                {
                    var ass = t.GetTypes().Where(x => x.GetCustomAttributes<TAttribute>().Any());
                    if (ass.IsNullOrEmpty()) continue;
                    classes = classes.IsNullOrEmpty() ? ass : classes.Union(ass);
                }
            }
            return classes;
        }

        #region 创建实例
        /// <summary>
        /// 创建实例
        /// 当前程序集和className 应在同一个dll文件中，所以应该把这一块东西拷贝到当前项目的代码里
        /// </summary>
        /// <typeparam name="T">可以是Interface</typeparam>
        /// <param name="className"></param>
        /// <returns></returns>
        private static T GetInstance<T>(string className)
            where T : class
        {
            Assembly assembly;
            object obj;
            Type type;

            try
            {
                assembly = Assembly.GetExecutingAssembly();
                type = assembly.GetType(className);
                obj = Activator.CreateInstance(type);
                ////或者直接这样写
                ////type = Type.GetType(className);
                ////obj = type.Assembly.CreateInstance(type.Name);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ":GetInstance(" + className + ")");
            }
            return obj as T;
        }

        /// <summary>
        /// 通过程序集文件名和类名获得对象
        /// </summary>
        /// <typeparam name="T">可以是Interface</typeparam>
        /// <param name="className"></param>
        /// <param name="assemblyFile"></param>
        /// <returns></returns>
        public static T GetInstance<T>(string className, string assemblyFile)
            where T : class
        {
            Assembly assembly;
            object obj;
            Type type;
            try
            {
                assembly = Assembly.Load(assemblyFile);
                type = assembly.GetType(className);
                obj = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ":GetInstance(" + className + "," + assemblyFile + ")");
            }
            return obj as T;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="assemblyFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object GetObject(string className, string assemblyFile)
        {
            Assembly assembly;
            object obj;
            Type type;
            try
            {
                assembly = Assembly.Load(assemblyFile);
                type = assembly.GetType(className);
                obj = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ":GetObject(" + className + "," + assemblyFile + ")");
            }
            return obj;
        }

        /// <summary>
        /// 通过程序及 类名 方法名 参数执行方法
        /// ReflectionHelper.DoMethod("WindowsFormsApplication.TestService","WindowsFormsApplication","GetStr", "a", "b");
        /// </summary>
        /// <param name="className"></param>
        /// <param name="assemblyFile"></param>
        /// <param name="method"></param>
        /// <param name="pas"></param>
        /// <returns></returns>
        public static object DoMethod(string className, string assemblyFile, string method, params object[] pas)
        {
            if (assemblyFile.IsNotNullOrEmpty())
            {
                object obj = GetObject(className, assemblyFile);
                Type type = obj.GetType();
                //根据方法名获取MethodInfo对象
                MethodInfo methodInfo = type.GetMethod(method);
                //参数1类型为object[]，代表Hello World方法的对应参数，输入值为null代表没有参数
                return methodInfo.Invoke(obj, pas);
            }
            else
            {
                ////当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type type = Type.GetType(className);
                object obj = type.Assembly.CreateInstance(type.Name);
                return obj;
            }
        }
        #endregion
    }
}
