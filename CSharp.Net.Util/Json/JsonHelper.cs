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
        /// 获取json对象列表
        /// </summary>
        /// <param name="input">json data</param>
        /// <param name="dataKey">默认空,如果<paramref name="input"/>是对象且需要指定key时可用该字段</param>
        /// <returns></returns>
        /// <exception cref="JsonFormatterException"></exception>
        public static List<Dictionary<string, string>> GetJList(string input, string dataKey = null)
        {
            var node = GetJsonNode(input);
            if (node is JsonValue)
                throw new JsonFormatterException("input is a value");

            Func<JsonArray, List<Dictionary<string, string>>> func = (array) =>
            {
                List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
                foreach (var arr in array)
                {
                    Dictionary<string, string> dt = new Dictionary<string, string>();
                    foreach (var o in (JsonObject)arr)
                        dt.Add(o.Key, o.Value.ToString());

                    data.Add(dt);
                }
                return data;
            };

            if (node is JsonObject)
            {
                foreach (var item in node.AsObject())
                    if (item.Value is JsonArray && (dataKey.IsNullOrEmpty() || dataKey == item.Key))
                        return func((JsonArray)item.Value);
            }
            else if (node is JsonArray)
            {
                return func(node.AsArray());
            }
            return null;
        }

        /// <summary>
        /// 获取json对象字段
        /// </summary>
        /// <param name="input">json data</param>
        /// <param name="dataKey">默认返回第一级对象,指定字段(多层级用:) xxx:xx</param>
        /// <returns>
        /// <para>返回dataFiled最末字段的值，</para>
        /// <para>如果对应的是对象则返回对象内所有字段，</para>
        /// <para>如果对应列表则返回第一条记录的所有字段。</para>
        /// </returns>
        public static Dictionary<string, string> GetJObject(string input, string dataKey = null)
        {
            var node = GetJsonNode(input);

#if NET6_0_OR_GREATER
            if (node is not JsonObject)
                throw new JsonFormatterException("input is not object");
#endif
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] fileds = dataKey?.Trim(':').Split(":");
            if (fileds.IsNullOrEmpty())
            {
                JsonObject obj = node.AsObject();
                foreach (var item in obj)
                {
                    if (data.ContainsKey(item.Key)) continue;
                    data.Add(item.Key, item.Value?.ToString());
                }
                return data;
            }

            JsonObject jobj = JsonNode.Parse(input).AsObject();

            for (int i = 0; i < fileds.Length; i++)
            {
                foreach (var item in jobj)
                {
                    if (item.Key != fileds[i]) continue;
                    if (item.Value is JsonObject)
                        jobj = (JsonObject)item.Value;

                    else if (item.Value is JsonArray)
                        jobj = (JsonObject)item.Value[0];

                    else if (item.Value is JsonValue)
                        data.Add(item.Key, item.Value.ToString());
                    break;
                }
            }
            if (data.IsNullOrEmpty())
            {
                foreach (var item in jobj)
                    data.Add(item.Key, item.Value.ToString());
            }
            return data;
        }

        /// <summary>
        /// 获取json中指定的字段
        /// </summary>
        /// <param name="input">json data</param>
        /// <param name="dataKey">指定字段 xxx:xx</param>
        /// <returns></returns>
        public static string GetJValue(string input, string dataKey)
        {
            string[] fileds = dataKey.Trim(':').Split(":");
            if (fileds.IsNullOrEmpty())
                return input;

            JsonObject jobj = JsonNode.Parse(input).AsObject();
            for (int i = 0; i < fileds.Length; i++)
            {
                foreach (var item in jobj)
                {
                    if (item.Key != fileds[i]) continue;
                    if (i == fileds.Length - 1)
                        return item.Value.ToString();

                    if (item.Value is JsonObject)
                        jobj = (JsonObject)item.Value;

                    else if (item.Value is JsonArray)
                        jobj = (JsonObject)item.Value[0];
                    else
                        throw new JsonFormatterException($"node {item.Key} not object or array,please check dataKey.");
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取jsonNode
        /// <para>使用方法，例：jsonNode["data"]["codes"][1].ToString();</para>
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JsonNode GetJsonNode(string json)
        {
            return JsonNode.Parse(json);
        }

        /// <summary>
        /// 判断是否json
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsJson(string input)
        {
            try
            {
                input = input.Trim();
                return input.StartsWith("{") && input.EndsWith("}")
                       || input.StartsWith("[") && input.EndsWith("]");
            }
            catch
            {
                return false;
            }
        }
    }
}
//#endif