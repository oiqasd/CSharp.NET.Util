using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 正则校验帮助类
    /// </summary>
    public class RegexHelper
    {
        /**
        * 方法命名规则 Check + 待校验参数英文名称
        * 例：校验手机号 CheckPhone 
        **/

        /// <summary>
        /// 校验手机号码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns>返回是否符合手机号格式</returns>
        public static bool CheckPhone(string phone)
        {
            var regex = new Regex(@"^(0|86|17951)?(13[0-9]|15[0-9]|16[0-9]|17[0-9]|18[0-9]|14[5678]|19[0-9])[0-9]{8}$");
            return regex.IsMatch(phone);
        }

        /// <summary>
        /// 校验手机号码，不包含国家编码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns>返回是否符合手机号格式</returns>
        public static bool CheckPhoneCommon(string phone)
        {
            var regex = new Regex(@"(13[0-9]|15[0-9]|16[0-9]|17[0-9]|18[0-9]|14[5678]|19[0-9])[0-9]{8}$");
            return regex.IsMatch(phone);
        }

        /// <summary>
        /// 验证手机号+电话号码
        /// </summary>
        /// <param name="number">号码</param>
        /// <returns></returns>
        public static bool CheckMobilePhone(string number)
        {
            string str = @"^1[\d]{10}$";
            string str1 = @"^([0-9]{3,4}-)?[0-9]{7,8}$";
            Regex reg = new Regex(str);
            Regex regtel = new Regex(str1);

            //手机号全是数字并以1开头 电话号码 XXXX-XXXXXXXX
            if (reg.IsMatch(number) || regtel.IsMatch(number))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 校验Email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>返回是否符合Email格式</returns>
        public static bool CheckEmail(string email)
        {
            Regex regex = new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            return regex.IsMatch(email);
        }

        /// <summary>
        /// 校验IP地址
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>返回是否符合Ip地址格式</returns>
        public static bool CheckIp(string ip)
        {
            var regex = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
            return regex.IsMatch(ip);
        }

        /// <summary>
        /// 校验正整数
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>返回是否为正整数</returns>
        public static bool CheckPositiveInteger(long number)
        {
            Regex regexPositiveInteger = new Regex(@"^[1-9][0-9]\d*$");
            return regexPositiveInteger.IsMatch(number.ToString());
        }

        /// <summary>
        /// 校验小数（正数）
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="digits">保留位数（默认保留1位小数，最多支持保留3位小数）</param>
        /// <returns>返回是否为小数</returns>
        public static bool CheckDecimalWithDigits(decimal? number, int digits = 1)
        {
            if (digits != 1 && digits != 2 && digits != 3)
                digits = 1;

            var regex = new Regex(@"^[0-9]+(.[0-9]{0," + digits + "})?$");
            return regex.IsMatch(number.ToString());
        }

        /// <summary>
        /// 校验日期格式（如2017-03-20 2017/03/20）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>是否符合日期（2017-03-20）格式</returns>
        public static bool RegexDate(string date)
        {
            Regex regex = new Regex(@"^\d{4}(-|/)\d{1,2}(-|/)\d{1,2}");
            return regex.IsMatch(date);
        }

        /// <summary>
        /// 校验证身份证号
        /// 推荐使用IDCardHelper.CheckIDCard
        /// </summary>
        /// <param name="cardno"></param>
        /// <returns></returns>
        public static bool CheckIDNumber(string cardno)
        {
            string str = @"^\d+$";
            Regex reg = new Regex(str);

            //身份证在10-18位
            if (cardno.Length > 10 && cardno.Length <= 18)
            {
                //截取字符串
                string subCardNo = cardno.Substring(0, cardno.Length - 1);
                //身份证全部是数字或最后一位是X
                if (reg.IsMatch(cardno) || (reg.IsMatch(subCardNo) && cardno.ToUpper().EndsWith("X")))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// 校验小数
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="digits">保留位数（默认保留1位小数，最多支持保留3位小数）</param>
        /// <param name="isNegative">是否支持负数</param>
        /// <returns>返回是否为小数</returns>
        public static bool CheckDecimalWithDigits(string number, int digits = 1, bool isNegative = false)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return false;
            }
            if (digits != 1 && digits != 2 && digits != 3)
                digits = 1;
            var reg = isNegative ? @"^(-)?[0-9]+(.[0-9]{0," + digits + "})?$" : @"^[0-9]+(.[0-9]{0," + digits + "})?$";
            var regex = new Regex(reg);
            return regex.IsMatch(number);
        }

        /// <summary>
        /// 校验数字
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool CheckNum(string num)
        {
            if (string.IsNullOrWhiteSpace((num)))
            {
                return false;
            }
            var regex = new Regex(@"^[0-9]*$");
            return regex.IsMatch(num);
        }

        /// <summary>
        /// 校验标点符合，目前只支持校验：-,，
        /// </summary>
        /// <param name="punctuation"></param>
        /// <returns></returns>
        public static bool CheckPunctuation(string punctuation)
        {
            if (string.IsNullOrWhiteSpace(punctuation))
            {
                return false;
            }
            var regex = new Regex(@"[-,，]");
            return regex.IsMatch(punctuation);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="strong"><para>校验强弱</para>
        /// <para>默认 False：6-16位,不能输入除字母和数字以外的字符，且必须包含数字和字母</para>
        /// True：8-16位,数字+字母+特殊符号组合
        /// </param>
        /// <returns>True:校验成功，False:失败 <para>ArgumentNullException</para></returns>
        public static bool CheckPwd(string pwd, bool strong = false)
        {
            if (string.IsNullOrEmpty(pwd))
            {
                throw new ArgumentNullException("密码不能为空");
            }
            else
            {
                string strRgx = string.Empty;
                if (!strong) strRgx = "^(?=.*?[a-zA-Z])(?=.*?[0-9])[a-zA-Z0-9]{6,16}$";
                else strRgx = "^(?=.*?[a-zA-Z])(?=.*?[0-9])(?=.*?[^\\w\\s]).{8,16}$";
                Regex regex = new Regex(strRgx);
                if (!regex.IsMatch(pwd))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取汉字列表
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> GetHanZiList(string text)
        {
            List<string> list = new List<string>();
            var regex = new Regex(@"[\u4e00-\u9fa5]+");
            foreach (var str in regex.Matches(text))
            {
                list.Add(str.ToString());
            }
            return list;
        }

        /// <summary>
        /// QueryString转Dictionany
        /// </summary>
        /// <param name="value"></param>
        /// <returns>value需自行转码,如:HttpUtility.UrlDecode(Value) <para>空返回NULL</para></returns>
        public static Dictionary<string, object> QueryStringToDictionany(string value)
        {
            if (string.IsNullOrEmpty(value)) { return null; }
            var data = Regex.Matches(value, "([^?=&]+)(=([^&]*))?")
                            .Cast<Match>()
                            .ToDictionary(x => x.Groups[1].Value, x => (object)x.Groups[3].Value);
            return data;
        }
    }
}

