using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Standard.Util
{
    /// <summary>
    /// Gets the <see cref="Utils"/> for help.
    /// </summary>
    public class Utils
    {
        #region 随机数生成
        /// <summary>
        /// 随机密码 前三数字+后三字母
        /// </summary>
        /// <returns></returns>
        public static string AutoPassword()
        {
            string[] s1 = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r",
                              "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
                              "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            var p = new Random().Next(999).ToString("D3");
            p += s1[new Random().Next(0, s1.Length)];
            p += s1[new Random().Next(0, s1.Length)];
            p += s1[new Random().Next(0, s1.Length)];
            return p;
        }

        /// <summary>
        /// 生成随机数
        /// 默认4位，支持1-10位
        /// </summary>
        /// <returns></returns>
        public static string GetRandom(int num = 4)
        {
            num = num <= 0 ? 1 : num;
            num = num >= 10 ? 10 : num;
            StringBuilder sbMax = new StringBuilder();

            for (int i = 0; i < num; i++)
            {
                sbMax.Append("9");
            }
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(0, int.Parse(sbMax.ToString())).ToString("D" + num.ToString());
        }

        /// <summary>
        /// 生成编号，19位
        /// str+yyMMddHHmmssfff+4位随机数
        /// </summary>
        public static string CreateOrderId(string headCode = "")
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
        #endregion

        #region 其它校验

        /// <summary>
        /// 检查时间戳是否过期
        /// </summary>
        /// <returns></returns>
        public static bool CheckTimeStamp(long timeStamp)
        {
            long nowTimeStamp = DateTimeHelper.GetTimeStamp();
            if (Math.Abs(nowTimeStamp - timeStamp) > 3000)
                return false;
            return true;
        }

        /// <summary>
        /// 检查手机验证码
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

        #region 身份证校验
        /// <summary>
        /// 检查身份证
        /// </summary>
        /// <param name="idcard"></param>
        /// <returns></returns>
        public static bool CheckIDCard(string idcard)
        {
            bool flag = false;

            if (idcard.Length == 18)
            {
                flag = CheckIDCard18(idcard);
            }
            else if (idcard.Length == 15)
            {
                flag = CheckIDCard15(idcard);
            }

            return flag;
        }

        /// <summary>
        /// 验证18位的身份证
        /// </summary>
        /// <param name="idcard"></param>
        /// <returns></returns>
        public static bool CheckIDCard18(string idcard)
        {
            long n = 0;
            if (long.TryParse(idcard.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(idcard.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idcard.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            string birth = idcard.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = idcard.Remove(17).ToCharArray();

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != idcard.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }

            return true;//符合GB11643-1999标准
        }

        /// <summary>
        /// 验证15位的身份证
        /// </summary>
        /// <param name="idcard"></param>
        /// <returns></returns>
        public static bool CheckIDCard15(string idcard)
        {
            long n = 0;
            if (long.TryParse(idcard, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idcard.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            string birth = idcard.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            return true;//符合15位身份证标准
        }

        /// <summary>
        /// 根据身份证得到出生日期
        /// </summary>
        /// <param name="hkid">身份证号码</param>
        /// <returns>出生日期</returns>
        public static DateTime GetBirthdayToHKID(string hkid)
        {
            DateTime date = DateTime.Now;
            if (hkid != null)
            {
                string docNo = hkid.Trim();
                string year = docNo.Substring(6, 4);
                string month = docNo.Substring(10, 2);
                string day = docNo.Substring(12, 2);
                date = DateTime.Parse(year + "-" + month + "-" + day);
            }
            return date;
        }

        /// <summary>
        /// 根据身份证得到性别
        /// </summary>
        /// <param name="hkid">身份证号码</param>
        /// <returns></returns>
        public static string GetSexToHKID(string hkid)
        {
            string gender = "男";
            if (hkid != null)
            {
                string sex = string.Empty;
                if (hkid.Length == 18)
                {
                    sex = hkid[hkid.Length - 2].ToString();
                }
                else
                {
                    sex = hkid[hkid.Length - 1].ToString();
                }
                if (Convert.ToInt32(sex) % 2 == 0)
                {
                    gender = "女";
                }
            }
            return gender;
        }

        /// <summary>
        /// 计算年龄
        /// </summary>
        /// <param name="dateOfBirth">出生日期</param>
        /// <param name="referenceDate">默认当前时间</param>
        /// <returns></returns>
        public static int CalculateAge(DateTime dateOfBirth, DateTime? referenceDate = null)
        {
            DateTime tmpdt = referenceDate ?? DateTime.Now;
            var years = tmpdt.Year - dateOfBirth.Year;
            if (tmpdt.Month < dateOfBirth.Month || (tmpdt.Month == dateOfBirth.Month && tmpdt.Day < dateOfBirth.Day))
                --years;
            return years;
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
        /// 将对象属性转换为key-value对
        /// </summary>
        /// <returns></returns>
        public static SortedDictionary<String, Object> EntityToMap(Object o)
        {
            SortedDictionary<String, Object> map = new SortedDictionary<string, object>();
            Type t = o.GetType();

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();
                if (mi != null && mi.IsPublic)
                {
                    map.Add(p.Name, mi.Invoke(o, new Object[] { }));
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
                var path = Path.Combine(SystemHelper.GetRunRoot, "logs");

                string file = FileHelper.GetFilePath(path, "log.txt", true);

                FileHelper.AppendWrittenFile(file,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + str);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExcetionMessage());
            }
        }
        #endregion
    }
}