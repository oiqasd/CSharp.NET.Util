//#if NET6_0_OR_GREATER

using CSharp.Net.Util.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        static JsonSerializerOptions _options { get; }

        /// <summary>
        /// 配置JsonSerializerOptions
        /// </summary>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns><paramref name="jsonSerializerOptions"/>
        /// <para>.PropertyNameCaseInsensitive : true</para>
        /// <para>.PropertyNamingPolicy : CamelCase</para>
        /// <para>.ReferenceHandler : IgnoreCycles</para>
        /// <para>.DefaultIgnoreCondition : WhenWritingNull</para>
        /// <para>.Encoder : UnsafeRelaxedJsonEscaping</para>
        /// <para>.IsoDateTimeConverter</para>
        /// <para>.IsoDateTimeOffsetConverter</para>
        /// <para>.VersionConverter</para>
        /// </returns>
        public static JsonSerializerOptions SetJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
        {
            if (jsonSerializerOptions == null) jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = false;//反序列化时不区分属性大小写
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;//使用 camel 大小写 
            jsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;//字典key驼峰
            jsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;// | JsonNumberHandling.WriteAsString,//允许或写入带引号的数字,例如:"23"
                                                                                             //保留引用并处理循环引用
            jsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            jsonSerializerOptions.WriteIndented = false; //格式化输出
                                                         //jsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;//允许有注释
            jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;//等同NullValueHandling.Ignore
                                                                                               //Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);//不unicode转换
            jsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            jsonSerializerOptions.Converters.Add(new IsoDateTimeConverter());
            jsonSerializerOptions.Converters.Add(new IsoDateTimeOffsetConverter());
            jsonSerializerOptions.Converters.Add(new VersionConverter());
            //jsonSerializerOptions.Converters.Add(new StringOrIntConverter());
            return jsonSerializerOptions;
        }
        static JsonHelper()
        {
            _options = SetJsonSerializerOptions(_options);
        }

        /// <summary>
        /// 将指定的对象序列化成 JSON 数据
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">自定义配置</param>
        /// <returns></returns>
        public static string Serialize(object obj, JsonSerializerOptions options = null)
        {
            if (null == obj) return null;
#if NET6_0_OR_GREATER
            return JsonSerializer.Serialize(obj, options ?? _options)!;
#else
            return JsonSerializer.Serialize(obj, options ?? _options);
#endif
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public string Serialize(object obj, Type type, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Serialize(obj, type, options ?? _options);
        }

        /// <summary>
        /// 将指定的 JSON 数据反序列化成指定对象。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">JSON 数据</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            return JsonSerializer.Deserialize<T>(json, _options);
        }

        /// <summary>
        /// 将指定的 JSON 数据反序列化成指定对象
        /// </summary>
        /// <typeparam name="T">对象集合</typeparam>
        /// <param name="json">JSON 数据</param>
        /// <returns></returns>
        public static List<T> DeserializeList<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(List<T>);

            return JsonSerializer.Deserialize<List<T>>(json, _options);
        }
        /// <summary>
        /// 将指定的 JSON 流数据反序列化成指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public T Deserialize<T>(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (TextReader reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                return Deserialize<T>(json);
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public object Deserialize(string json, Type type, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize(json, type, options ?? _options)!;
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
        /// <returns>value 为 string</returns>
        public static Dictionary<string, string> GetJObject(string jsonData)//<T>
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            JsonObject obj = JsonNode.Parse(jsonData).AsObject();
            foreach (var item in obj)
            {
                if (data.ContainsKey(item.Key)) continue;
                //if (item.Value is JsonObject || item.Value is JsonArray)
                //    data.Add(item.Key, ConvertHelper.ConvertTo<T>(item.Value.ToJsonString()));
                //else if(typeof(T) is object)
                data.Add(item.Key, item.Value?.ToString());
            }
            return data;
        }

        /// <summary>
        /// 获取jsonNode
        /// <para>使用方法，例：jsonNode["data"]["codes"][1].ToString();</para>
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public JsonNode GetJsonNode(string json)
        {
            return JsonNode.Parse(json);
        }
    }
}
//#endif