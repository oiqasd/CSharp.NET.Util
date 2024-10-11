using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 进程锁
    /// </summary>
    public class MutexLockManager
    {
        private Mutex GetMutexForKey(string key)
        {
            string mutexName = $"{key}";
            return new Mutex(false, mutexName);
        }
        public static async void ExecuteWithLock(string key, Action action)
        {
            // 获取与key关联的Mutex
            //using (Mutex mutex = GetMutexForKey(key))
            using (Mutex mutex = new Mutex(false, key))
            {
                try
                {
                    // 等待获取Mutex锁
                    mutex.WaitOne();
                    // 执行需要同步的代码
                    action();
                }
                finally
                {
                    // 释放Mutex锁
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
