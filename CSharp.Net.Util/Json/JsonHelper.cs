#if NET6_0_OR_GREATER

using CSharp.Net.Util.Json;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 使用System.Text.Json序列化
    /// Newtonsoft.Json 迁移文档
    /// https://docs.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0
    /// </summary>
    public class JsonHelper
    {
        static JsonSerializerOptions _serializeOptions;

        static JsonHelper()
        {
            _serializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,//反序列化时不区分属性大小写
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,//使用 camel 大小写 
                NumberHandling = JsonNumberHandling.AllowReadingFromString,// | JsonNumberHandling.WriteAsString,//允许或写入带引号的数字,例如:"23"
                //ReferenceHandler = ReferenceHandler.Preserve,//保留引用并处理循环引用 
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                //WriteIndented = true, //格式化输出
                //ReadCommentHandling = JsonCommentHandling.Skip,//允许有注释
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,//等同NullValueHandling.Ignore
                //Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),//不unicode转换
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            _serializeOptions.Converters.Add(new IsoDateTimeOffsetConverter());
            _serializeOptions.Converters.Add(new IsoDateTimeConverter());
            _serializeOptions.Converters.Add(new VersionConverter());
            //serializeOptions.Converters.Add(new StringOrIntConverter());
        }

        /// <summary>
        /// 将指定的对象序列化成 JSON 数据
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">自定义配置</param>
        /// <returns></returns>
        public static string Serialize(object obj, JsonSerializerOptions options = null)
        {
            try
            {
                if (null == obj) return null;
                return JsonSerializer.Serialize(obj, options ?? _serializeOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将指定的 JSON 数据反序列化成指定对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="json">JSON 数据。</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);
            try
            {
                return JsonSerializer.Deserialize<T>(json, _serializeOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将指定的 JSON 数据反序列化成指定对象。
        /// </summary>
        /// <typeparam name="T">对象集合</typeparam>
        /// <param name="json">JSON 数据</param>
        /// <returns></returns>
        public static List<T> DeserializeList<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(List<T>);
            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, _serializeOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ///// <summary>
        ///// 将转换后的Key全部设置为小写
        ///// </summary>
        ///// <param name="json"></param>
        ///// <returns></returns>
        //public static SortedDictionary<string, object> DeserializeLower(string json)
        //{
        //    if (json.IsNullOrEmpty()) return new SortedDictionary<string, object>();
        //    var obj = Deserialize<SortedDictionary<string, object>>(json);
        //    SortedDictionary<string, object> nobj = new SortedDictionary<string, object>();

        //    foreach (var item in obj)
        //    {
        //        nobj[item.Key.ToLower()] = item.Value;
        //    }
        //    obj.Clear();
        //    obj = null;
        //    return nobj;
        //}

        /// <summary>
        /// 将value中的 双引号替换为中文双引号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToJsonString(string str)
        {
            char[] tempArr = str.ToCharArray();
            int tempLength = tempArr.Length;
            for (int i = 0; i < tempLength; i++)
            {
                if (tempArr[i] == ':' && tempArr[i + 1] == '"')
                {
                    for (int j = i + 2; j < tempLength; j++)
                    {
                        if (tempArr[j] == '"')
                        {
                            if (tempArr[j + 1] != ',' && tempArr[j + 1] != '}')
                            {
                                tempArr[j] = '”'; // 将value中的 双引号替换为中文双引号
                            }
                            else if (tempArr[j + 1] == ',' || tempArr[j + 1] == '}')
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return new string(tempArr);
        }

        /// <summary>
        /// 动态获取json数据,在不确定key场景可用此方法
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static Dictionary<string, T> GetJObject<T>(string jsonData)
        {
            Dictionary<string, T> data = new Dictionary<string, T>();
            JsonObject obj = JsonNode.Parse(jsonData).AsObject();
            foreach (var item in obj)
            {
                if (data.ContainsKey(item.Key)) continue;
                if (item.Value is JsonObject || item.Value is JsonArray)
                    data.Add(item.Key, ConvertHelper.ConvertTo<T>(item.Value.ToJsonString()));
                else
                    data.Add(item.Key, item.Value.GetValue<T>());
            }
            return data;
        }

    }
}
#endif