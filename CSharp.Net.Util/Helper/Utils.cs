using CSharp.Net.Util.Cryptography;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// Gets the <see cref="Utils"/> for help.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 总62位 a-zA-Z0-9
        /// </summary>
        const string characters_all = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        /// <summary>
        /// 36位 0-9A-Z
        /// </summary>
        const string characters_upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

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
            //StringBuilder sbMax = new StringBuilder();             
            //for (int i = 0; i < length; i++) 
            //    sbMax.Append("9"); 
#if NET6_0_OR_GREATER
            return Random.Shared.Next(0, (int)Math.Pow(10, length));
#else
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(0, (int)Math.Pow(10, length));
#endif
        }

        /// <summary>
        /// 生成随机数值
        /// </summary>
        /// <param name="min">最小(含)</param>
        /// <param name="max">最大(不含)</param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
#if NET6_0_OR_GREATER
            return Random.Shared.Next(min, max);
#else
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(min, max);
#endif
        }

        /// <summary>
        /// 生成编号，19位
        /// str+yyMMddHHmmssfff+4位随机数
        /// </summary>
        [Obsolete("有重复风险，建议使用CreateWorkId")]
        public static string CreateOrderId(string headCode = null)
        {
            //System.Threading.Thread.Sleep(1);
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
        /// <para>自定义配置方式：IdWorkerHelper.GeneratorIdWorker(IdWorkerOptions options)</para>
        /// </summary>
        public static long CreateWorkId(DateTime? baseTime = null)
        {
            if (IdWorkerHelper.Instance == null)
            {
                IdWorkerHelper.GeneratorIdWorker();
            }
            return IdWorkerHelper.Instance.NextId(baseTime);
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

        /// <summary>
        /// 生成短链接key
        /// 0.03%的概率会重复
        /// </summary>
        /// <param name="data"></param>
        /// <returns>4段任选一段，区分大小写</returns>
        [Obsolete]
        public static string[] ShortKey(string data)
        {
            //自定义生成MD5加密字符传前的混合KEY
            string key = "yvdh";
            //对传入数据进行MD5加密
            string hex = MD5.Md5Encrypt(key + data);
            string[] resShortData = new string[4];
            for (int i = 0; i < 4; i++)
            {
                //把加密字符按照8位一组16进制与0x3FFFFFFF进行位与运算
                int hexint = 0x3FFFFFFF & Convert.ToInt32("0x" + hex.Substring(i * 8, 8), 16);
                string outChars = string.Empty;
                for (int j = 0; j < 6; j++)
                {
                    //把得到的值与0x0000003D进行位与运算，取得字符数组chars索引
                    int index = 0x0000003D & hexint;
                    //把取得的字符相加
                    outChars += characters_all[index];
                    //每次循环按位右移5位
                    hexint = hexint >> 5;
                }
                //把字符串存入对应索引的输出数组
                resShortData[i] = outChars;
            }
            return resShortData;
        }

        /// <summary>
        /// 生成一个不重复的短字符串
        /// </summary>
        /// <returns></returns>
        public static string ShortCode(DateTime? baseTime = null)
        {
            long code = Utils.CreateWorkId(baseTime ?? DateTime.Parse("2024/07/01"));
            return NumberUtil.DecimalToBinary(code, 62);
        }

        #endregion

        #region 其它校验

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
#if NET8_0_OR_GREATER
            Random.Shared.Shuffle(arr);
            return arr;
#else
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
#endif
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
        /// <param name="fileName">写入的文件,默认当前目录'logs/log.txt'下</param> 
        public static async Task WriteLog(string str, string fileName = null)
        {
            try
            {
                var path = Path.Combine(AppDomainHelper.GetRunRoot, "logs");
                string file = fileName ?? FileHelper.GetFilePath(path, "log.txt", true);
                await FileHelper.AppendWrittenFile(file, str);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.GetExcetionMessage());
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
#if NET6_0_OR_GREATER
            Random random = Random.Shared;
#else
            Random random = new Random(DateTimeHelper.GetTimeStampInt());
#endif
            double money = random.NextDouble() * remainMoney / remainSize * 2;
            money = Math.Floor(money);
            return (int)money;
        }

        /// <summary>
        /// 计算代码执行耗时
        /// </summary>
        /// <param name="action"></param>
        /// <returns><see cref="long"/></returns>
        public static long CalcExecuteTime(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}