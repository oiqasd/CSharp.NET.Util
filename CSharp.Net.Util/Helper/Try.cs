using CSharp.Net.Util;
using System;
using System.Threading.Tasks;

public struct Try
{
    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="defaultValue"></param>
    /// <returns>T</returns>
    public static T Func<T>(Func<T> func, T defaultValue = default(T))//where T : class
    {
        try
        {
            return func();
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static async Task<T> Func<T>(Func<Task<T>> func, T defaultValue = default(T))
    {
        try
        {
            return await func.Invoke();
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <param name="func"></param>
    /// <param name="errorAction"></param>
    /// <returns>true:成功,false:异常</returns>
    public static T Func<T>(Func<T> func, Action<Exception> errorAction)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            if (errorAction != null) errorAction(ex);
            return default(T);
        }
    }
    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func">异步委托</param>
    /// <param name="catchAction"></param>
    /// <returns></returns>
    public static async Task<T> Func<T>(Func<Task<T>> func, Func<Task<T>> catchAction)
    {
        try
        {
            return await func.Invoke();
        }
        catch
        {
            return await catchAction.Invoke();
        }
    }

    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="func">执行</param>
    /// <param name="catchFunc">catch时执行</param>
    /// <returns>T</returns>
    public static T Func<T>(Func<T> func, Func<T> catchFunc)
    {
        try
        {
            return func();
        }
        catch
        {
            return catchFunc();
        }
    }

    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <param name="action"></param>
    /// <param name="throwException">是否throw</param>
    /// <returns>true:成功,false:异常</returns>
    public static bool Action(Action action, bool throwException = false)
    {
        try
        {
            action();
            return true;
        }
        catch
        {
            if (throwException)
                throw;
            return false;
        }
    }
    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <param name="action"></param>
    /// <param name="errorAction">是否throw</param>
    /// <returns>true:成功,false:异常</returns>
    public static bool Action(Action action, Action<Exception> errorAction)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Fatal(ex);
            if (errorAction != null) errorAction(ex);            
            return false;
        }
    }

    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <param name="action"></param>
    /// <param name="args"></param>
    /// <param name="errorAction">是否throw</param>
    /// <returns>true:成功,false:异常</returns>
    public static bool Action(Action<object> action,object args, Action<Exception> errorAction)
    {
        try
        {
            action(args);
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Fatal(ex);
            if (errorAction != null) errorAction(ex);
            return false;
        }
    }
}

