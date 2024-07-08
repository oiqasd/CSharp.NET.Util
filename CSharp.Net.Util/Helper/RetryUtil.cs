using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 重试类
    /// </summary>
    public sealed class RetryUtil
    {
        /// <summary>
        /// 重试有异常的方法
        /// </summary>
        /// <param name="action"></param>
        /// <param name="numRetries">重试次数</param>
        /// <param name="retryTimeout">重试间隔时间</param>
        /// <param name="finalThrow">是否最终抛异常</param>
        /// <param name="exceptionTypes">终止重试异常类型,可多个</param>
        public static async Task Invoke(Action action, uint numRetries = 10, int retryTimeout = 1000, bool finalThrow = true, Type[] exceptionTypes = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            await Invoke(async () =>
             {
                 action();
                 await Task.CompletedTask;
             }, numRetries, retryTimeout, finalThrow, exceptionTypes);
        }

        /// <summary>
        /// 重试有异常的方法，还可以指定特定异常
        /// </summary>
        /// <param name="action"></param>
        /// <param name="numRetries">重试次数</param>
        /// <param name="retryTimeout">重试间隔时间</param>
        /// <param name="finalThrow">是否最终抛异常</param>
        /// <param name="exceptionTypes">终止重试异常类型,可多个</param>
        public static async Task Invoke(Func<Task> action, uint numRetries, int retryTimeout = 1000, bool finalThrow = true, Type[] exceptionTypes = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            do
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Console.WriteLine($"You can retry {numRetries} more times.");

                    LogHelper.Error(ex, $"You can retry {numRetries} more times.");

                    if (numRetries <= 0)
                    {
                        if (finalThrow) throw;
                        else return;
                    }

                    // 如果填写了 exceptionTypes 且异常类型不在 exceptionTypes 之内，则终止重试
                    if (exceptionTypes != null && exceptionTypes.Length > 0 && !exceptionTypes.Any(u => u.IsAssignableFrom(ex.GetType())))
                    {
                        if (finalThrow) throw;
                        else return;
                    }
                    // 如果可重试异常数大于 0，则间隔指定时间后继续执行
                    if (retryTimeout > 0) await Task.Delay(retryTimeout);
                }
            } while (numRetries-- > 0);
        }

        /// <summary>
        /// 重试有异常的方法，还可以指定特定异常
        /// </summary>
        /// <param name="action"></param>
        /// <param name="numRetries">重试次数,默认1次</param>
        /// <param name="retryTimeout">重试间隔时间</param>
        /// <param name="finalThrow">是否最终抛异常</param>
        /// <param name="exceptionTypes">终止重试异常类型,可多个</param>
        public static async Task<object> Invoke(Func<Task<object>> action, uint numRetries = 1, int retryTimeout = 1000, bool finalThrow = true, Type[] exceptionTypes = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            do
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Console.WriteLine($"You can retry {numRetries} more times.");

                    LogHelper.Error(ex, $"You can retry {numRetries} more times.");

                    // 如果可重试次数小于或等于0，则终止重试
                    if (numRetries <= 0)
                    {
                        if (finalThrow) throw;
                        else return null;
                    }

                    // 如果填写了 exceptionTypes 且异常类型不在 exceptionTypes 之内，则终止重试
                    if (exceptionTypes != null && exceptionTypes.Length > 0 && !exceptionTypes.Any(u => u.IsAssignableFrom(ex.GetType())))
                    {
                        if (finalThrow) throw;
                        else return null;
                    }

                    // 如果可重试异常数大于 0，则间隔指定时间后继续执行
                    if (retryTimeout > 0) await Task.Delay(retryTimeout);
                }
            } while (numRetries-- > 0);
            return null;
        }
    }
}
