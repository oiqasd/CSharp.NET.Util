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
        bool IsTheRawGenericType(Type type) => generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
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
        => ConvertHelper.ConvertTo(obj, defaultValue);

    /// <summary>
    /// 转成Long
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defaultValue">0</param>
    /// <returns></returns>
    public static long ToLong(this object obj, long defaultValue = 0)
        => ConvertHelper.ConvertTo(obj, defaultValue);

    /// <summary>
    /// 转成Boolean
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="defaultValue">false</param>
    /// <returns></returns>
    public static bool ToBoolean(this object obj, bool defaultValue = false)
        => ConvertHelper.ConvertTo(obj, defaultValue);

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
    /// <param name="move">0：代表无意义</param>
    /// <returns>true:包含</returns>
    public static bool CheckBitwise(this int value, byte move)
    => Utils.CheckBitwise((uint)value, move);

    /// <summary>
    /// 计算位或后数值，
    /// 适用于一对多的场景
    /// </summary>
    /// <param name="source">初始值从0开始</param>
    /// <param name="move">0~31,0：无意义</param>
    /// <returns>source | <![CDATA[1<<move-1]]></returns>
    public static int ToBitOr(this int source, byte move)
    {
        if (move > 31) throw new ArgumentException("pow must be less than 32.");
        return (int)Utils.CalcBitwiseValue((uint)source, move);
    }

}