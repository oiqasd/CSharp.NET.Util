using CSharp.Net.Util.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 钉钉消息
    /// https://open.dingtalk.com/document/robots/custom-robot-access
    /// </summary>
    public class DingTalkerRobotUtil
    {
        //SEC65b7ab709a2be9b4cd11c698a0a7cb0cc5822241922791e8691f79ecf54fa6d0
        //https://oapi.dingtalk.com/robot/send?access_token=72817cf5f4f76b9a5f294aefa971555de63683d2aea13a8dfe9e5567a8416e41

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendTextMsg(string message, string token = null, string secret = null) => await SendTextMsg(message, null, token, secret);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="atMobile"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendTextMsg(string message, string[] atMobile = null, string token = null, string secret = null)
        {
            if (message.IsNullOrEmpty() || token.IsNullOrEmpty() || secret.IsNullOrEmpty())
                throw new ArgumentNullException("message,token,secret can't null");

            long timestamp = DateTimeHelper.GetTimeStampLong();
            string sigdata = $"{timestamp}\n{secret}";
            string sign = HMAC.HmacSHA256_Base64(sigdata, secret);

            string encodeSign = StringHelper.UrlEncodeUTF_8(sign);
            string url = $"https://oapi.dingtalk.com/robot/send?access_token={token}&timestamp={timestamp}&sign={encodeSign}";
            string data = "{\"text\": {\"content\":\"" + message + "\"},\"msgtype\":\"text\"}";
            //string data = "{\"at\": {\"atMobiles\":[\"15392067032\"],\"isAtAll\": false},\"text\": {\"content\":\"{message}\"},\"msgtype\":\"text\"}";
            var ret = await HttpClientUtil.PostAsync(url, data);
            return ret;
        }

        /// <summary>
        /// 发送markdown消息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendMarkDownMsg(string title, string message, string token = null, string secret = null) => await SendMarkDownMsg(title, message, null, token, secret);

        /// <summary>
        /// 发送markdown消息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="atMobile"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendMarkDownMsg(string title, string message, string[] atMobile = null, string token = null, string secret = null)
        {
            if (message.IsNullOrEmpty() || token.IsNullOrEmpty() || secret.IsNullOrEmpty())
                throw new ArgumentNullException("message,token,secret can't null");

            long timestamp = DateTimeHelper.GetTimeStampLong();
            string sigdata = $"{timestamp}\n{secret}";
            string sign = HMAC.HmacSHA256_Base64(sigdata, secret);

            string encodeSign = StringHelper.UrlEncodeUTF_8(sign);
            string url = $"https://oapi.dingtalk.com/robot/send?access_token={token}&timestamp={timestamp}&sign={encodeSign}";

            var text = "{\"msgtype\": \"markdown\",\"markdown\": {\"title\":\"" + title + "\",\"text\": \"" + message + " \n\"}}";
            //var text = "{\"msgtype\": \"markdown\",\"markdown\": {\"title\":\"杭州天气\",\"text\": \"xxx\n\"},\"at\": {\"atMobiles\":[\"150XXXXXXXX\"],\"atUserIds\": [\"user123\"],\"isAtAll\": false}}";

            var ret = await HttpClientUtil.PostAsync(url, text);
            return ret;//{"errcode":0,"errmsg":"ok"}
        }
    }
}
