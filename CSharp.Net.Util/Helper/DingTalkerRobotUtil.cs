using CSharp.Net.Util.Cryptography;
using System;
using System.Collections.Generic;
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

            long timestamp = DateTimeHelper.GetTimestamp();
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
        public static async Task<string> SendMarkDownMsg(string title, string message, string token, string secret, PostFileDto fileDto = null, params string[] atMobile)
        {
            if (message.IsNullOrEmpty() || token.IsNullOrEmpty() || secret.IsNullOrEmpty())
                throw new ArgumentNullException("message,token,secret can't null");

            long timestamp = DateTimeHelper.GetTimestamp();
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

        static async Task<string> SendActionCard(string title, string message, string token = null, string secret = null, params string[] atMobile)
        {
            if (message.IsNullOrEmpty() || token.IsNullOrEmpty() || secret.IsNullOrEmpty())
                throw new ArgumentNullException("message,token,secret can't null");

            long timestamp = DateTimeHelper.GetTimestamp();
            string sigdata = $"{timestamp}\n{secret}";
            string sign = HMAC.HmacSHA256_Base64(sigdata, secret);

            string encodeSign = StringHelper.UrlEncodeUTF_8(sign);
            string url = $"https://oapi.dingtalk.com/robot/send?access_token={token}&timestamp={timestamp}&sign={encodeSign}";
            /*
            string ms = "#### 项目任务分配与进度\r\n" +
                "| 任务名称 | 负责人 | 进度 | 截止日期 |\r\n" +
                "|-------|-------|-------|-------|\r\n" +
                "| 需求分析 | 张三   | 100% | 2025-10-20 |\r\n" +
                "| 开发编码 | 李四   | 70%  | 2025-11-10 |\r\n" +
                "| 测试验收 | 王五   | 0%   | 2025-11-20 |\r\n\r\n" +
                "> 点击下方按钮查看详细进度报表";
            */
            var text = "{\"msgtype\": \"actionCard\",\"actionCard\": {\"title\":\"" + title + "\",\"text\": \"" + message
                       + "\n\"},\"btnOrientation\":\"1\",\"buttons\":[{\"title\":\"详情\",\"actionURL\":\"https://www.baidu.com\"}]}";

            var ret = await HttpClientUtil.PostAsync(url, text);
            return ret;//{"errcode":0,"errmsg":"ok"}
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secret"></param>
        /// <param name="fileDto"></param>
        /// <returns></returns>
        public static async Task<string> SendFile(string token, string secret, PostFileDto fileDto)
        {
            string url = $"https://oapi.dingtalk.com/media/upload?access_token={token}&type=image";
            var ret = await HttpClientUtil.PostFileAsync(url, fileDto);
            Console.WriteLine(ret);

            return JsonHelper.GetFieldValue(ret, "media_id");
        }

        public static async Task<string> GetAccessToken(string appkey, string secret)
        {
            string ret = await HttpClientUtil.GetAsync($"https://oapi.dingtalk.com/gettoken?appkey={appkey}&appsecret={secret}");
            return JsonHelper.GetFieldValue(ret, "access_token");
        }
    }
}
