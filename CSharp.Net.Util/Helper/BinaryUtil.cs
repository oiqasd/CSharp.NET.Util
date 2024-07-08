using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CSharp.Net.Util
{
    public class BinaryUtil
    {
        /// <summary>
        /// 字符串转2进制
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string String2BinaryString(string input)
        {
            StringBuilder binaryBuilder = new StringBuilder();
            foreach (char c in input)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(new char[] { c });
                string binaryString = Convert.ToString(bytes[0], 2).PadLeft(8, '0'); // 转换为8位二进制字符串
                binaryBuilder.Append(binaryString);
            }
            return binaryBuilder.ToString();
        }

        /// <summary>
        /// 字符串转10进制
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long String2DeciamlString(string input)
        {
            string b = String2BinaryString(input);
            return Convert.ToInt64(b, 2);
        }
        /// <summary>
        /// 字符串转16进制
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string String2Hex(string input)
        {
            StringBuilder hex = new StringBuilder(input.Length * 2);
            foreach (char c in input)
                hex.AppendFormat("{0:X2}", (int)c);
            return hex.ToString();
        }

        /// <summary>
        /// 字符串进制转换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="b">默认62，最大86</param>
        /// <returns></returns>
        public static string StringToBaseX(string input, byte b = 62)
        { 
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            BigInteger bigInt = new BigInteger(bytes);
            return NumberUtil.StringToBaseX(bigInt, b);
        }
    }
}
