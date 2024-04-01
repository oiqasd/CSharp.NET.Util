using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET
using System.Runtime.Loader;
#endif
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public class ReflectUtil
    {
        /// <summary>
        /// 获取入口程序集
        /// </summary>
        /// <returns></returns>
        public static Assembly GetEntryAssembly()
            => Assembly.GetEntryAssembly();

#if NET
        /// <summary>
        /// 根据程序集名称获取运行时程序集
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
            => AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
#endif
        /// <summary>
        /// 根据路径加载程序集
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static Assembly LoadAssembly(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            return Assembly.LoadFrom(path);
        }

        /// <summary>
        /// 通过流加载程序集
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(MemoryStream assembly)
            => Assembly.Load(assembly.ToArray());

        /// <summary>
        /// 根据程序集和类型名获取运行时类型
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeFullName">类型完全名获</param>
        /// <returns></returns>
        public static Type GetType(Assembly assembly, string typeFullName)
            => assembly.GetType(typeFullName);

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyName(Assembly assembly)
            => assembly.GetName().Name;

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetAssemblyName(Type type)
            => GetAssemblyName(type.GetTypeInfo());

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public static string GetAssemblyName(TypeInfo typeInfo)
            => GetAssemblyName(typeInfo.Assembly);

        /// <summary>
        /// 获取调用链方法名
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string GetCallingMethodName(int depth = 1)
            => new StackTrace().GetFrame(depth)?.GetMethod()?.Name;
        /*获取当前
          System.Reflection.MethodBase.GetCurrentMethod().Name;
          Assembly.GetExecutingAssembly().GetName().Name
        */


        /// <summary>
        /// 获取调用链类名
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string GetCallingClassName(int depth = 1)
            => new StackTrace().GetFrame(depth)?.GetMethod()?.DeclaringType?.ToString();
        //System.Reflection.Assembly.GetCallingAssembly();
        //获取当前
        //this.GetType().Name


        /// <summary>
        /// 判断值类型或者无参构造函数
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool HasDefaultConstructor(Type t)
            => t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;

        /// <summary>
        /// 获取私有构造函数
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ConstructorInfo[] GetConstructorInfo(Type t)
            => t.GetConstructors().OrderBy(x => x.GetParameters().Length - x.GetParameters().Count(p => p.IsOptional)).ToArray();
        //return t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null); 

        /// <summary>
        /// 反射获取所有带有 <typeparamref name="TAttribute"/> 特性的类
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
        /// ReflectionHelper.InvokeMethod("WindowsFormsApplication.TestService","WindowsFormsApplication","GetStr", "a", "b");
        /// </summary>
        /// <param name="className"></param>
        /// <param name="assemblyFile"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object InvokeMethod(string className, string assemblyFile, string method, params object[] args)
        {
            if (assemblyFile.IsNotNullOrEmpty())
            {
                object obj = GetObject(className, assemblyFile);
                Type type = obj.GetType();
                //type.InvokeMember(method, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Instance, BindingFlags.Public, BindingFlags.NonPublic, null, obj, args);
                //根据方法名获取MethodInfo对象
                MethodInfo methodInfo = type.GetMethod(method);
                //参数1类型为object[]，代表Hello World方法的对应参数，输入值为null代表没有参数
                return methodInfo.Invoke(obj, args);
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

    /// <summary>
    /// 反射实体类
    /// </summary>
    /// <typeparam name="TargetClass"></typeparam>
    public class ReflectHelper<TargetClass>
    {
        private TargetClass mTarget;
        private NestObject mNestObject;

        /// <summary>
        /// 要反射的对象
        /// </summary>
        public TargetClass Target
        {
            get { return mTarget; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">要反射的对象</param>
        public ReflectHelper(TargetClass target)
        {
            mTarget = target;
            mNestObject = new NestObject(target);
        }

        /// <summary>
        /// 执行指定的方法
        /// </summary>
        /// <param name="method">目标方法的名称</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public object InvokeMethod(string method, object[] args)
        {
            return mTarget.GetType().InvokeMember(
                method,
                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, mTarget, args);
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="field">字段名称(层次结构可以用.分隔)</param>
        /// <returns>字段值</returns>
        public object GetFieldValue(string field)
        {
            mNestObject.SetField(field);
            return mNestObject.FieldInfo.GetValue(mNestObject.FieldParent);
        }

        /// <summary>
        /// 设置字段值
        /// </summary>
        /// <param name="field">字段名称(层次结构可以用.分隔)</param>
        /// <param name="value"></param>
        public void SetField(string field, object value)
        {
            mNestObject.SetField(field);
            mNestObject.FieldInfo.SetValue(mNestObject.FieldParent, value);
        }
    }

    internal class NestObject
    {
        private const int NEST_MAX = 10;

        private object mTarget;
        private FieldInfo mFieldInfo;
        private object mFieldParent;
        private int mNest;

        public FieldInfo FieldInfo
        {
            get { return mFieldInfo; }
        }

        public object FieldParent
        {
            get { return mFieldParent; }
        }

        public NestObject(object target)
        {
            mTarget = target;
        }

        public void SetField(string field)
        {
            mFieldInfo = null;
            mFieldParent = null;
            SetFieldByNestedField(field, mTarget);
            Console.WriteLine("SetField field=" + field + ", mFieldInfo=" + mFieldInfo + ", mFieldParent=" + mFieldParent);
        }

        private void SetFieldByNestedField(string hierarchy, object target)
        {
            Console.WriteLine("Called SetFieldByNestedField, hierarchy=" + hierarchy + ", target.GetType().Name=" + target.GetType().Name);

            NestName names = new NestName(hierarchy);
            if (names.IsNextNames() == false)
            {
                //无上层字段名称
                SetFieldOrThrow(names.FirstName, target);
            }
            else
            {
                if (names.IsFreeParent())
                {
                    //指定任意一个层次时

                    SetFieldByFreeParent(names.NextNames, target);
                }
                else if (names.IsFreeHierarchy())
                {
                    //指定了任意多个上层
                    string tmpNames = names.NextNames;
                    for (int i = 0; i < NEST_MAX; i++)
                    {
                        // 尝试用临时名称调用SetFieldByNestedField()
                        // 如果找不到字段，则尝试将其命名为深入层次
                        if (TryFieldByNestedField(tmpNames, target))
                        {
                            return;
                        }
                        else
                        {
                            tmpNames = "*." + tmpNames;
                        }
                    }
                    throw new ApplicationException("未找到字段.hierarchy=" + hierarchy);
                }
                else
                {
                    // 指定特定层次
                    object obj = GetObjectOrThrow(names.FirstName, target);
                    SetFieldByNestedField(names.NextNames, obj);
                }
            }
        }

        private bool TryFieldByNestedField(string hierarchy, object target)
        {
            try
            {
                SetFieldByNestedField(hierarchy, target);
            }
            catch
            {
                return false;
            }
            return true;
        }


        private void SetFieldOrThrow(string field, object target)
        {
            Console.WriteLine("Called SetFieldOrThrow, field=" + field + ", target.GetType().Name=" + target.GetType().Name);

            mFieldInfo = target.GetType().GetField(field,
                BindingFlags.GetField | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            mFieldParent = target;

            if (mFieldInfo == null)
            {
                throw new ApplicationException("未找到字段.field=" + field);
            }
        }

        private void SetFieldByFreeParent(string nextNames, object target)
        {
            Console.WriteLine("Called SetFieldByFreeParent, nextNames=" + nextNames + ", target.GetType().Name=" + target.GetType().Name);

            FieldInfo[] fieldInfoArr = target.GetType().GetFields(
                BindingFlags.GetField | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo fieldInfo in fieldInfoArr)
            {
                Console.WriteLine("fieldInfo.Name=" + fieldInfo.Name);

                // 尝试用临时名称调用SetFieldByNestedField()
                object obj = fieldInfo.GetValue(target);
                if (TryFieldByNestedField(nextNames, obj))
                {
                    return;
                }
            }

            throw new ApplicationException("未找到字段.nextNames=" + nextNames);
        }

        /// <summary>
        /// 获取指定的对象
        /// </summary>
        /// <param name="hierarchy">字段名称（可.分隔符中指定层次）</param>
        /// <returns>对象</returns>
        public object GetObject(string hierarchy)
        {
            mNest = 0;
            return GetObjectByNestedField(hierarchy, mTarget);
        }

        private object GetObjectOrThrow(string field, object target)
        {
            FieldInfo fieldInfo = target.GetType().GetField(field,
                BindingFlags.GetField | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfo.GetValue(target);
        }

        private object GetObjectByNestedField(string hierarchy, object target)
        {
            Console.WriteLine("Called GetObjectByNestedField, hierarchy=" + hierarchy + ", target.GetType().Name=" + target.GetType().Name);

            if (mNest >= NEST_MAX)
            {
                return null;
            }
            mNest++;

            NestName names = new NestName(hierarchy);
            if (names.IsNextNames() == false)
            {
                return GetObjectOrThrow(names.FirstName, target);
            }
            else
            {
                if (names.IsFreeParent())
                {
                    return GetNextObjectByFreeParent(names.NextNames, target);
                }
                else if (names.IsFreeHierarchy())
                {
                    return GetNextObjectByFreeHierarchy(names.NextNames, target);
                }
                else
                {
                    object obj = GetObjectOrThrow(names.FirstName, target);
                    return GetObjectByNestedField(names.NextNames, obj);
                }
            }
        }

        private object GetNextObjectByFreeParent(string nextNames, object target)
        {
            FieldInfo[] fieldInfoArr = target.GetType().GetFields(
                BindingFlags.GetField | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo fieldInfo in fieldInfoArr)
            {
                Console.WriteLine("fieldInfo.Name=" + fieldInfo.Name);
                try
                {
                    object obj = GetObjectByNestedField(nextNames, fieldInfo);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
                catch { }
            }

            return null;
        }

        private object GetNextObjectByFreeHierarchy(string nextNames, object target)
        {
            Console.WriteLine("Called GetNextObjectByFreeHierarchy, nextNames=" + nextNames);

            FieldInfo[] fieldInfoArr = target.GetType().GetFields(
                BindingFlags.GetField | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // 查找该层是否存在指定字段
            foreach (FieldInfo fieldInfo in fieldInfoArr)
            {
                Console.WriteLine("fieldInfo.Name=" + fieldInfo.Name);
                if (fieldInfo.Name.Equals(nextNames))
                {
                    return fieldInfo.GetValue(target);
                }
            }

            // 检查底层是否存在指定字段
            foreach (FieldInfo fieldInfo in fieldInfoArr)
            {
                Console.WriteLine("fieldInfo.Name=" + fieldInfo.Name);
                try
                {
                    object obj = GetObjectByNestedField(nextNames, fieldInfo);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
                catch { }
            }

            return null;
        }
    }

    /// <summary>
    /// 使用.分隔的名称(字段名称)的类
    /// </summary>
    internal class NestName
    {
        private string mFirstName;
        private string mNextNames;
        private string mUpperNames;
        private string mLastName;

        /// <summary>
        /// .分隔符中的起始名称
        /// </summary>
        public string FirstName
        {
            get { return mFirstName; }
        }

        /// <summary>
        /// .分隔符中的第二个后缀的名称
        /// </summary>
        public string NextNames
        {
            get { return mNextNames; }
        }

        /// <summary>
        /// .分隔符中的最后一个名称
        /// </summary>
        public string LastName
        {
            get { return mLastName; }
        }

        /// <summary>
        /// .分隔符中从开头到结尾的名字
        /// </summary>
        public string UpperNames
        {
            get { return mUpperNames; }
        }

        public NestName(string names)
        {
            SetFirstAndNextNames(names);
            SetUpperAndLastNames(names);
        }

        private void SetFirstAndNextNames(string names)
        {
            string[] nameArr = names.Split(new char[] { '.' }, 2);
            mFirstName = nameArr[0];
            if (nameArr.Length >= 2)
            {
                mNextNames = nameArr[1];
            }
        }

        public bool IsNextNames()
        {
            return mNextNames != null;
        }

        public bool IsFreeParent()
        {
            return "*".Equals(mFirstName);
        }

        public bool IsFreeHierarchy()
        {
            return "**".Equals(mFirstName);
        }

        private void SetUpperAndLastNames(string names)
        {
            int index = names.LastIndexOf('.');
            if (index < 0)
            {
                mUpperNames = null;
                mLastName = names;
            }
            else
            {
                mUpperNames = names.Substring(0, index);
                mLastName = names.Substring(index + 1);
            }
        }
    }
}
