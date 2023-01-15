using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util.Helper
{
    /// <summary>
    /// https://www.cnblogs.com/wei325/p/16065342.html
    /// </summary>
    class ThreadHelper
    {
        /// <summary>
        /// CAS原子操作加减 替换lock
        /// </summary>
        public static void AtomicityForInterLock()
        {
            long result = 0;
            Console.WriteLine("开始计算");
            Parallel.For(0, 10, (i) =>
            {
                for (int j = 0; j < 10000; j++)
                {
                    //自增
                    Interlocked.Increment(ref result);

                    //Interlocked主要函数如下：
                    //Interlocked.Increment 原子操作，递增指定变量的值并存储结果。
                    //Interlocked.Decrement 原子操作，递减指定变量的值并存储结果。
                    //Interlocked.Add 原子操作，添加两个整数并用两者的和替换第一个整数
                    //Interlocked.Exchange 原子操作，赋值
                    //Interlocked.CompareExchange(ref a, b, c); 原子操作，a参数和c参数比较， 相等b替换a，不相等不替换。方法返回值始终是第一个参数的原值，也就是内存里的值
                }
            });
            Console.WriteLine($"结束计算");
            Console.WriteLine($"result正确值应为：{10000 * 10}");
            Console.WriteLine($"result    现值为：{result}");
            Console.ReadLine();
        }

        //读写锁
        private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static int index = 0;
        static void read()
        {
            try
            {
                //进入读锁
                rwl.EnterReadLock();
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"线程id:{Thread.CurrentThread.ManagedThreadId},读数据,读到index:{index}");
                }
            }
            finally
            {
                //退出读锁
                rwl.ExitReadLock();
            }
        }
        static void write()
        {
            try
            {
                //尝试获写锁
                while (!rwl.TryEnterWriteLock(50))
                {
                    Console.WriteLine($"线程id:{Thread.CurrentThread.ManagedThreadId},等待写锁");
                }
                Console.WriteLine($"线程id:{Thread.CurrentThread.ManagedThreadId},获取到写锁");
                for (int i = 0; i < 5; i++)
                {
                    index++;
                    Thread.Sleep(50);
                }
                Console.WriteLine($"线程id:{Thread.CurrentThread.ManagedThreadId},写操作完成");
            }
            finally
            {
                //退出写锁
                rwl.ExitWriteLock();
            }
        }
        /// <summary>
        /// 执行多线程读写
        /// </summary>
        static void test()
        {
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
            Task[] task = new Task[6];
            task[1] = taskFactory.StartNew(write); //写
            task[0] = taskFactory.StartNew(read); //读
            task[2] = taskFactory.StartNew(read); //读
            task[3] = taskFactory.StartNew(write); //写
            task[4] = taskFactory.StartNew(read); //读
            task[5] = taskFactory.StartNew(read); //读

            for (var i = 0; i < 6; i++)
            {
                task[i].Wait();
            }

        }
    }
}
