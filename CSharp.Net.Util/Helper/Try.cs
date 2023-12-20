using System;

public class Try
{
    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="action">执行委托</param>
    /// <param name="defaultValue">返回默认值</param>
    /// <returns>T</returns>
    public static T CatchOrDefault<T>(Func<T> action, T defaultValue = default(T))//where T : class
    {
        try
        {
            return action();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 委托异常处理
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="action">执行委托</param>
    /// <param name="catchAction">catch时执行</param>
    /// <returns>T</returns>
    public static T CatchOrDefault<T>(Func<T> action, Func<T> catchAction)
    {
        try
        {
            return action();
        }
        catch
        {
            return catchAction();
        }
    }
}

