using CSharp.Net.Util.Cryptography;
using System;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 钉钉消息
    /// https://open.dingtalk.com/document/robots/custom-robot-access
    /// https://open.dingtalk.com/document/orgapp/custom-bot-send-message-type
    /// </summary>
    public class DingTalkerRobotUtil
    {
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendTextMsg(string message, string token = null, string secret = null) => await SendTextMsg(message, token, secret, null);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="atMobile"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendTextMsg(string message, string token = null, string secret = null, params string[] atMobile)
        {
            if (message.IsNullOrEmpty() || token.IsNullOrEmpty() || secret.IsNullOrEmpty())
                throw new ArgumentNullException("message,token,secret can't null");

            long timestamp = DateTimeHelper.GetTimeStamp();
            string sigdata = $"{timestamp}\n{secret}";
            string sign = HMAC.HmacSHA256_Base64(sigdata, secret);

            string encodeSign = StringHelper.UrlEncodeUTF_8(sign);
            string url = $"https://oapi.dingtalk.com/robot/send?access_token={token}&timestamp={timestamp}&sign={encodeSign}";
            //string data = "{\"text\": {\"content\":\"" + message + "\"},\"msgtype\":\"text\"}";
            string data = "{\"at\": {\"atMobiles\":" + JsonHelper.Serialize(atMobile ?? new string[0]) + ",\"isAtAll\": false},\"text\": {\"content\":\"" + message + "\"},\"msgtype\":\"text\"}";
            var ret = await HttpClientUtil.PostAsync(url, data);
            return ret;
        }

        /// <summary>
        /// 发送markdown消息，移动端不支持表格显示
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendMarkDownMsg(string title, string message, string token = null, string secret = null)
            => await SendMarkDownMsg(title, message, token, secret, null);

        /// <summary>
        /// 发送markdown消息,移动端不支持表格显示
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="atMobile">menssage里也要@这个号码</param>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static async Task<string> SendMarkDownMsg(string title, string message, string token = null, string secret = null, params string[] atMobile)
        {
            if (message.IsNullOrEmpty() || token.IsNullOrEmpty() || secret.IsNullOrEmpty())
                throw new ArgumentNullException("message,token,secret can't null");

            long timestamp = DateTimeHelper.GetTimeStamp();
            string sigdata = $"{timestamp}\n{secret}";
            string sign = HMAC.HmacSHA256_Base64(sigdata, secret);

            string encodeSign = StringHelper.UrlEncodeUTF_8(sign);
            string url = $"https://oapi.dingtalk.com/robot/send?access_token={token}&timestamp={timestamp}&sign={encodeSign}";

            //var text = "{\"msgtype\": \"markdown\",\"markdown\": {\"title\":\"" + title + "\",\"text\": \"" + message + " \n\"}}";
            //var text = "{\"msgtype\": \"markdown\",\"markdown\": {\"title\":\"杭州天气\",\"text\": \"xxx\n\"},\"at\": {\"atMobiles\":[\"150XXXXXXXX\"],\"atUserIds\": [\"user123\"],\"isAtAll\": false}}";
            var text = "{\"msgtype\": \"markdown\",\"markdown\": {\"title\":\"" + title + "\",\"text\": \"" + message
                        + "\n\"},\"at\": {\"atMobiles\":" + JsonHelper.Serialize(atMobile ?? new string[0])
                        + ",\"atUserIds\": [],\"isAtAll\": false}}";

            var ret = await HttpClientUtil.PostAsync(url, text);
            return ret;//{"errcode":0,"errmsg":"ok"}
        }
    }
}
