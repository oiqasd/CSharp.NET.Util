

namespace CSharp.Net.Util
{
    /// <summary>
    /// 通用单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonHelper<T> where T : new()
    {
        private static object LockKey = new object();

        public static T _Instance;

        /// <summary>
        /// 获取并创建单例
        /// </summary>
        /// <returns></returns>
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (LockKey)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new T();
                        }
                    }
                }
                return _Instance;
            }

        }
    }
}