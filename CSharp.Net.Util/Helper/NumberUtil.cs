using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 金钱转换
    /// </summary>
    public class MoneyUtil
    {
        /// <summary>
        /// 转大写
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static string ToUpper(decimal money)
        {
            return NumberUtil.NumberString(money);
        }
    }
    /// <summary>
    /// 数字转换大写汉字
    /// (金额)
    /// </summary>
    public class NumberUtil
    {
        #region 数字转换大写汉字
        private static string[] NumChineseCharacter = new string[] { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        /// <summary>
        /// 数字转换成大写汉字主函数
        /// </summary>
        /// <returns>返回转换后的大写汉字</returns>
        public static string NumberString(decimal number)
        {
            string bb = string.Empty;
            string xs = "0";
            string Num = "";
            bool IsNegative = false; // 是否是负数

            if (number < 0)
            {
                // 是负数则先转为正数
                number = Math.Abs(number);
                IsNegative = true;
            }

            var m = number.ToString();
            if (m.Contains("."))
            {
                string[] ss = m.Split('.');
                xs = ss[1];
                Num = ss[0];
            }
            else
            {
                Num = m;
            }

            var arr = ConvertString(Num).ToCharArray();
            Array.Reverse(arr);
            for (int i = 0; i < arr.Length; i++)
            {
                switch (i % 4)
                {
                    case 1:
                        bb = arr[i] + "拾" + bb;
                        break;
                    case 2:
                        bb = arr[i] + "百" + bb;
                        break;
                    case 3:
                        bb = arr[i] + "仟" + bb;
                        break;
                    default:
                        if (i == 0)
                        {
                            bb = arr[i].ToString();
                        }
                        else if (i == 8)
                        {
                            bb = arr[i] + "亿" + bb;
                        }
                        else
                        {
                            bb = arr[i] + "万" + bb;
                        }
                        break;
                }
            }

            bb = bb + "圆";
            if (int.Parse(xs) > 0)
            {
                var arrx = ConvertString(xs).ToCharArray();

                for (int i = 0; i < arrx.Length; i++)
                {
                    bb = bb + arrx[i];
                    if (i == 0)
                    {
                        bb = bb + "角";
                    }
                    else if (i == 1)
                    {
                        bb = bb + "分";
                    }
                }
            }
            else
            {
                bb = bb + "整";
            }

            if (IsNegative == true)
            {
                return "负" + bb;
            }

            return bb;
        }

        /// <summary>
        /// 小写数字转大写
        /// </summary>
        /// <param name="Num"></param>
        /// <returns></returns>
        public static string ConvertString(string Num)
        {
            string bb = "";
            for (int i = 0; i < Num.Length; i++)
            {
                bb += NumChineseCharacter[int.Parse(Num.Substring(i, 1))];
            }
            return bb;
        }

        /// <summary>
        /// 将全角数字转换为数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SBCCaseToNumberic(string value)
        {
            char[] c = value.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                byte[] b = System.Text.Encoding.Unicode.GetBytes(c, i, 1);
                if (b.Length == 2)
                {
                    if (b[1] == 255)
                    {
                        b[0] = (byte)(b[0] + 32);
                        b[1] = 0;
                        c[i] = System.Text.Encoding.Unicode.GetChars(b)[0];
                    }
                }
            }
            return new string(c);
        }
        #endregion

        /// <summary>
        /// 62进制转换
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string EncodeHexadecimal(long num)
        {
            string characters_all = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            StringBuilder stringBuilder = new StringBuilder();

            while (num > 0)
            {
                stringBuilder.Append(characters_all[(int)(num % 62)]);
                num = num / 62;
            }
            StringHelper.Reverse(stringBuilder);

            return stringBuilder.ToString();
        }

    }
}