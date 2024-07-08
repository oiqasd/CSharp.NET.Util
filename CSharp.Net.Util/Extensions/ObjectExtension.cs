using CSharp.Net.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class ObjectExtension
{
    /// <summary>
    /// 获取对象下的属性值
    /// </summary>
    /// <typeparam name="T">转换类型</typeparam>
    /// <param name="Obj"></param>
    /// <param name="colum">字段名</param>
    /// <returns></returns>
    public static T GetProperty<T>(this object Obj, string colum)
    {
        return GetProperty<T>(Obj, colum, default(T));
    }

    /// <summary>
    /// 获取对象下的属性值
    /// </summary>
    /// <typeparam name="T">转换类型</typeparam>
    /// <param name="Obj"></param>
    /// <param name="colum">字段名</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T GetProperty<T>(this object Obj, string colum, T defaultValue)
    {
        if (Obj == null)
            return defaultValue;
        object _obj;

        try
        {
            _obj = Obj.GetType().GetProperty(colum).GetValue(Obj, null);
        }
        catch (NullReferenceException e)
        {
            throw new Exception(string.Format("{0} The {1} does not exist in {2}", e.Message, colum, Obj.GetType().Name));
        }
        return ConvertHelper.ConvertTo<T>(_obj);
    }
#if NET6_0_OR_GREATER
    /// <summary>
    /// 合并两个字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic">字典</param>
    /// <param name="newDic">新字典</param>
    public static void AddOrUpdate<T>(this ConcurrentDictionary<string, T> dic, Dictionary<string, T> newDic)
    {
        foreach (var (key, value) in newDic)
        {
            dic.AddOrUpdate(key, value, (key, old) => value);
        }
    }
#endif

    /// <summary>
    /// 判断方法是否是异步
    /// </summary>
    /// <param name="method">方法</param>
    /// <returns></returns>
    public static bool IsAsync(this MethodInfo method)
    {
        return method.GetCustomAttribute<AsyncMethodBuilderAttribute>() != null
            || method.ReturnType.ToString().StartsWith(typeof(Task).FullName);
    }

    /// <summary>
    /// 获取方法真实返回类型
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static Type GetRealReturnType(this MethodInfo method)
    {
        // 判断是否是异步方法
        var isAsyncMethod = method.IsAsync();

        // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
        var returnType = method.ReturnType;
        return isAsyncMethod ? (returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void)) : returnType;
    }

    /// <summary>
    /// 判断类型是否实现某个泛型
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="generic">泛型类型</param>
    /// <returns>bool</returns>
    public static bool IsImplementedRawGeneric(this Type type, Type generic)
    {
        // 检查接口类型
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType) return true;

        // 检查类型
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            type = type.BaseType;
        }

        return false;
        bool IsTheRawGenericType(Type _type) => generic == (_type.IsGenericType ? _type.GetGenericTypeDefinition() : _type);
    }

    /// <summary>
    /// 将对象转换为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T ToType<T>(this object obj) => ConvertHelper.ConvertTo<T>(obj);

    /// <summary>
    /// 转成Int
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defaultValue">0</param>
    /// <returns></returns>
    public static int ToInt(this object obj, int defaultValue = 0)
    {
        if (int.TryParse(obj.ToString(), out int r))
            return r;
        return ConvertHelper.ConvertTo(obj, defaultValue);
    }

    /// <summary>
    /// 转成Long
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defaultValue">0</param>
    /// <returns></returns>
    public static long ToLong(this object obj, long defaultValue = 0)
    {
        if (long.TryParse(obj.ToString(), out long r))
            return r;
        return ConvertHelper.ConvertTo(obj, defaultValue);
    }

    /// <summary>
    /// 转成Boolean
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defaultValue">false</param>
    /// <returns></returns>
    public static bool ToBoolean(this object obj, bool defaultValue = false)
    {
        if (bool.TryParse(obj.ToString(), out bool r))
            return r;
        return ConvertHelper.ConvertTo(obj, defaultValue);
    }

    /// <summary>
    /// 转成DateTime
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defaultValue">false</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this object obj, DateTime defaultValue = default)
    {
        if (DateTime.TryParse(obj.ToString(), out DateTime r))
            return r;
        return ConvertHelper.ConvertTo(obj, defaultValue);
    }

    /// <summary>
    /// byte 数组保存到文件
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static void SaveToFile(this byte[] bytes, string fileName)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// 比较是否位与后是否相等
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index">基码，0：无意义</param>
    /// <returns>true:包含</returns>
    public static bool BitEqual(this int value, byte index)
    => Operators.BitEqual((uint)value, index);

    /// <summary>
    /// 比较是否位与后是否相等
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index">基码，0：无意义</param>
    /// <returns>true:包含</returns>
    public static bool BitEqual(this int value, int index)
    => Operators.BitEqual((uint)value, (byte)index);

    /// <summary>
    /// 计算位或后数值
    /// </summary>
    /// <param name="source">初始值从0开始</param>
    /// <param name="index">基码，0~31,0：无意义</param>
    /// <returns>source | <![CDATA[1<<index-1]]></returns>
    public static int BitOr(this int source, byte index)
    {
        if (index > 31) throw new ArgumentException("pow must be less than 32.");
        return (int)Operators.BitsValue((uint)source, index);
    }
    /// <summary>
    /// 计算位或后数值
    /// </summary>
    /// <param name="source">初始值从0开始</param>
    /// <param name="index">基码，0~31,0：无意义</param>
    /// <returns>source | <![CDATA[1<<index-1]]></returns>
    public static int BitOr(this int source, int index)
    {
        if (index > 31) throw new ArgumentException("pow must be less than 32.");
        return (int)Operators.BitsValue((uint)source, (byte)index);
    }

    /// <summary>
    /// 判断泛型是否List
    /// </summary>
    /// <typeparam name="T">必须是泛型</typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsList<T>(T t)
    {
        if (t.GetType().IsGenericTypeDefinition && typeof(List<>).IsAssignableFrom(t.GetType().GetGenericTypeDefinition()))
            return true;
        return false;
    }
}