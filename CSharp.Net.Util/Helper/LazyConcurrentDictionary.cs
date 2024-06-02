using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 线程安全ConcurrentDictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LazyConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> concurrentDictionary;
        public LazyConcurrentDictionary()
        {
            this.concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var lazyResult = this.concurrentDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
            // (1）None = 0【线程不安全】
            //（2）PublicationOnly = 1【针对于多线程，有多个线程运行初始化方法时，当第一个线程完成时其值则会设置到其他线程】
            //（3）ExecutionAndPublication = 2【针对单线程，加锁机制，每个初始化方法执行完毕，其值则相应的输出】

            return lazyResult.Value;
        }
    }


    class Test
    {
        private static int _runCount = 0;
        private static readonly LazyConcurrentDictionary<string, string> _lazyDictionary
              = new LazyConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {

            var task1 = Task.Run(() => PrintValue("1"));
            var task2 = Task.Run(() => PrintValue("2"));
            Task.WaitAll(task1, task2);

            PrintValue("JeffckyWang from cnblogs");
            Console.WriteLine(string.Format("运行次数为：{0}", _runCount));
            Console.Read();
        }

        static void PrintValue(string valueToPrint)
        {
            var valueFound = _lazyDictionary.GetOrAdd("key",
                 x =>
                 {
                     Interlocked.Increment(ref _runCount);
                     Thread.Sleep(100);
                     return valueToPrint;
                 });
            Console.WriteLine(valueFound);
        }


        void test()
        {
            var lazyBlog = new Lazy<Blog>
             (
                 () =>
                 {
                     var blogObj = new Blog() { BlogId = 100 };
                     return blogObj;
                 }, LazyThreadSafetyMode.PublicationOnly
             );
            ThreadPool.QueueUserWorkItem(new WaitCallback(Run), lazyBlog);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Run), lazyBlog);
        }

        void Run(object obj)
        {
            var blogLazy = obj as Lazy<Blog>;
            var blog = blogLazy.Value as Blog;
            blog.BlogId++;
            Thread.Sleep(100);
            Console.WriteLine("id：" + blog.BlogId);

        }
        class Blog
        {
            public int BlogId { get; set; }
        }
    }

}
