﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// Gets the <see cref="Utils"/> for help.
    /// </summary>
    public class Utils
    {
        #region 随机数生成
        /// <summary>
        /// 随机字符串 数字+大小写字母
        /// </summary>
        /// <returns></returns>
        public static string GetRandomString(int length = 6)
        {
            if (length <= 0) return "";

            string[] s1 = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                            //"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
                            // "n", "o", "p", "q", "r","s", "t", "u", "v", "w", "x", "y", "z", 
                            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J","K", "L", "M",
                            "N","O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            List<string> newList = GetRandomList(s1);

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                builder.Append(newList[new Random(Guid.NewGuid().GetHashCode()).Next(0, newList.Count)]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// 生成随机数字
        /// 默认4位，支持1-10位
        /// </summary>
        /// <returns></returns>
        public static int GetRandom(int length = 4)
        {
            length = length <= 0 ? 1 : length;
            length = length >= 10 ? 10 : length;
            StringBuilder sbMax = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                sbMax.Append("9");
            }
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(0, int.Parse(sbMax.ToString()));
        }

        /// <summary>
        /// 生成随机数值
        /// </summary>
        /// <param name="min">最小(含)</param>
        /// <param name="max">最大(不含)</param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(min, max);
        }

        /// <summary>
        /// 生成编号，19位
        /// str+yyMMddHHmmssfff+4位随机数
        /// </summary>
        public static string CreateOrderId(string headCode = null)
        {
            System.Threading.Thread.Sleep(1);
            StringBuilder orderId = new StringBuilder();
            //生成4位随机数
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            string iRand = rand.Next(1000, 9999).ToString();
            string strTime = DateTime.Now.ToString("yyMMddHHmmssfff");
            if (string.IsNullOrEmpty(headCode))
                return orderId.Append(strTime).Append(iRand).ToString();
            else
                return orderId.Append(headCode).Append(strTime).Append(iRand).ToString();

        }

        /// <summary>
        /// 雪花算法生成编号
        /// 自定义配置方式IdWorkerHelper.GeneratorIdWorker(IdWorkerOptions options)
        /// </summary>
        public static long CreateWorkId()
        {
            if (IdWorkerHelper.IdWorkInstance == null)
            {
                IdWorkerHelper.GeneratorIdWorker();
            }
            return IdWorkerHelper.IdWorkInstance.nextId();
        }

        /// <summary>  
        /// 根据GUID获取15位的唯一数字序列  
        /// </summary>  
        /// <returns></returns>  
        public static long GuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            long num = BitConverter.ToInt64(buffer, 0);
            return num.ToString().Substring(0, 15).ToLong();
        }
        #endregion

        #region 其它校验

        /// <summary>
        /// 检查时间戳是否过期
        /// 默认检查3s
        /// </summary>
        /// <returns></returns>
        public static bool CheckTimeStamp(long timeStamp, int intervalSeconds = 3000)
        {
            long nowTimeStamp = DateTimeHelper.GetTimeStampInt();
            if (Math.Abs(nowTimeStamp - timeStamp) > intervalSeconds)
                return false;
            return true;
        }

        /// <summary>
        /// 检查6位纯数字手机验证码
        /// </summary>
        /// <param name="vercode"></param>
        public static bool CheckMobileVerCode(string vercode)
        {
            bool flag = true;
            if (vercode.Length == 6)
            {
                //验证数字
                char[] code = vercode.ToCharArray();
                for (int i = 0; i < code.Length; i++)
                {
                    if (!Char.IsNumber(code[i]))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            else
                flag = false;
            return flag;
        }
        #endregion

        #region 其它功能
        /// <summary>
        /// 随机排序数组
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int[] GetRandomArray(int[] arr)
        {
            int[] arr2 = new int[arr.Length];

            for (int i = 0; i <= arr.Length - 1; i++)
            {
                Random rd = new Random();
                int r = arr.Length - i;
                int pos = rd.Next(r);
                arr2[i] = arr[pos];
                arr[pos] = arr[r - 1];
            }

            return arr2;
        }

        /// <summary>
        /// 随机排序列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> GetRandomList<T>(IEnumerable<T> list)
        {
            Random random = new Random();
            List<T> newList = new List<T>();
            foreach (T item in list)
            {
                newList.Insert(random.Next(newList.Count + 1), item);
            }
            return newList;
        }
        /// <summary>
        /// 将对象属性转换为key-value对
        /// </summary>
        /// <returns></returns>
        public static SortedDictionary<string, object> EntityToMap(object o)
        {
            SortedDictionary<string, object> map = new SortedDictionary<string, object>();
            Type t = o.GetType();

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();
                if (mi != null && mi.IsPublic)
                {
                    map.Add(p.Name, mi.Invoke(o, new object[] { }));
                }
            }
            return map;
        }

        /// <summary>
        /// 记录文本日志
        /// 记录在程序根目录下
        /// </summary>
        /// <param name="str">日志内容</param> 
        public static void WriteLog(string str)
        {
            try
            {
                var path = Path.Combine(AppDomainHelper.GetRunRoot, "logs");

                string file = FileHelper.GetFilePath(path, "log.txt", true);

                FileHelper.AppendWrittenFile(file, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + str);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExcetionMessage());
            }
        }
        #endregion

        /// <summary>
        /// 实时拆红包
        /// </summary>
        /// <param name="remainSize">剩余个数</param>
        /// <param name="remainMoney">剩余金额(单位分)</param>
        /// <returns></returns>
        public static int RedPacket(int remainSize, int remainMoney)
        {
            if (remainSize == 1)
                return remainMoney;

            Random random = new Random();
            double money = random.NextDouble() * remainMoney / remainSize * 2;

            money = Math.Floor(money);
            return (int)money;
        }
    }
}