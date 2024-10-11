using CSharp.Net.Util.Json;
using CSharp.Net.Util.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CSharp.Net.Util
{
    // 使用System.Text.Json序列化
    // Newtonsoft.Json 迁移文档
    // https://docs.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0
    /// <summary> 
    /// Json序列化工具
    /// <para>
    /// net8新增特性，可以反序列化只读字段或属性，此工具类未全局应用
    ///  若要选择此全局支持，请将新选项 PreferredObjectCreationHandling 设置为 JsonObjectCreationHandling.Populate。
    ///  如果考虑兼容性问题，还可通过将 [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)] 特性放置在要填充其属性的特定类型上或单个属性上来更精细地启用该功能。
    /// </para>
    /// </summary>
    public sealed class JsonHelper
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
        /// <para>.StringToIntegerConverter</para>
        /// <para>.StringToLongConverter</para>
        /// <para>.NumberToStringConverter</para>
        /// </returns>
        public static JsonSerializerOptions SetJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
        {
            if (jsonSerializerOptions == null) jsonSerializerOptions = new JsonSerializerOptions();
            //反序列化时不区分属性大小写
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            //使用 camel 大小写 
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            //字典key驼峰
            jsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            // | JsonNumberHandling.WriteAsString,//允许或写入带引号的数字,例如:"23"
            //保留引用并处理循环引用
            jsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            //格式化输出
            jsonSerializerOptions.WriteIndented = false;
            //是否允许末尾多余逗号
            jsonSerializerOptions.AllowTrailingCommas = false;
            //忽略只读属性
            jsonSerializerOptions.IgnoreReadOnlyProperties = false;
            //忽略值为Null的属性,等同NullValueHandling.Ignore    
            jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            //是否允许有注释，Skip:允许
            jsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Disallow;
            jsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            //Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);//不unicode转换,解决乱码问题
            jsonSerializerOptions.Converters.Add(new IsoDateTimeConverter());
            jsonSerializerOptions.Converters.Add(new IsoDateTimeOffsetConverter());
            jsonSerializerOptions.Converters.Add(new VersionConverter());
            jsonSerializerOptions.Converters.Add(new StringToIntegerConverter());
            jsonSerializerOptions.Converters.Add(new StringToLongConverter());
            jsonSerializerOptions.Converters.Add(new NumberToStringConverter());
#if !NET6_0
            //jsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
#endif
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
        public static string Serialize(object obj, Type type, JsonSerializerOptions options = null)
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
            //判断是否List
            //if(typeof(List<>).IsAssignableFrom(typeof(T).GetGenericTypeDefinition()))
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
                return default;
            return JsonSerializer.Deserialize<List<T>>(json, _options);
        }

        /// <summary>
        /// 将指定的 JSON 流数据反序列化成指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream)
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
        public static object Deserialize(string json, Type type, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize(json, type, options ?? _options);
        }
        /*
        /// <summary>
        /// 将转换后的Key全部设置为小写
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static SortedDictionary<string, object> DeserializeLower(string json)
        {
            if (json.IsNullOrEmpty()) return new SortedDictionary<string, object>();
            var obj = Deserialize<SortedDictionary<string, object>>(json);
            SortedDictionary<string, object> nobj = new SortedDictionary<string, object>();

            foreach (var item in obj)
            {
                nobj[item.Key.ToLower()] = item.Value;
            }
            obj.Clear();
            obj = null;
            return nobj;
        }
        */

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
        /// <para>获取json对象列表</para>
        /// 本方法可减少反复拆装箱操作因此有一定局限性,可使用<see cref="GetObject"/>方法自行转换
        /// <para>取对象下的数组使用<paramref name="dataKey"/>字段</para>
        /// </summary>
        /// <param name="jsonData">json data</param>
        /// <param name="dataKey">默认空,适用于对象下的某个字段是数组</param>
        /// <returns>Null or empty return null</returns>
        /// <exception cref="JsonFormatterException"></exception>
        public static List<Dictionary<string, object>> GetList(string jsonData, string dataKey = null)
        {
            var node = GetJsonNode(jsonData);
            if (node is JsonValue)
                throw new JsonFormatterException("input is a value");
            Func<JsonArray, List<Dictionary<string, object>>> funArr = null;
            Func<JsonObject, Dictionary<string, object>> funObj = null;
            funObj = (obj) =>
            {
                var dic = new Dictionary<string, object>();
                foreach (var item in obj)
                {
                    dic.Add(item.Key, item.Value);
                }
                return dic;
            };
            funArr = (array) =>
            {
                if (array.IsNullOrEmpty()) return null;
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                foreach (var arr in array)
                {
                    Dictionary<string, object> dt = new Dictionary<string, object>();
                    foreach (var o in (JsonObject)arr)
                        if (o.Value is JsonObject)
                            dt.Add(o.Key, funObj((JsonObject)o.Value));
                        else
                            dt.Add(o.Key, o.Value.ToValue());
                    data.Add(dt);
                }
                return data;
            };
            if (node is JsonObject)
            {
                foreach (var item in node.AsObject())
                    if (item.Value is JsonArray && (dataKey.IsNullOrEmpty() || dataKey == item.Key))
                        return funArr((JsonArray)item.Value);
            }
            else if (node is JsonArray)
            {
                return funArr(node.AsArray());
            }
            return null;
        }

        /// <summary>
        /// jsonData转Object
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns>AS <![CDATA[string,decimal,bool,object,Dictionary<string, object>,List<object>]]></returns>
        public static object GetObject(string jsonData)
            => ToObject(JsonDocument.Parse(jsonData).RootElement);

        /// <summary>
        /// 获取对象字典
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetObjectDict(string jsonData)
            => GetObject(jsonData) as Dictionary<string, object>;

        public static JsonObject GetJObject(string jsonData)
           => JsonNode.Parse(jsonData).AsObject();

        /// <summary>
        /// 获取json中指定的字段
        /// </summary>
        /// <param name="input">json data</param>
        /// <param name="field">指定字段 xxx:xx</param>
        /// <returns></returns>
        public static string GetFieldValue(string input, string field)
        {
            string[] fileds = field.Trim(':').Split(":");
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
                        throw new JsonFormatterException($"Node '{item.Key}' not object or array,please check dataKey.");
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取jsonNode
        /// <para>使用方法，例：jsonNode["data"]["codes"][1].GetValue<![CDATA[<]]>int>();</para>
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
#if NET5_0_OR_GREATER
                using var doc = JsonDocument.Parse(input);
                return true;
#endif
                input = input.Trim();
                int type = 0;
                if (input.StartsWith("{") && input.EndsWith("}"))
                    type = 1;
                else if (input.StartsWith("[") && input.EndsWith("]"))
                    type = 2;
                return type > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// jsonElement 转 Object
        /// </summary>
        /// <param name="jsonElement"></param>
        /// <returns></returns>
        internal static object ToObject(JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    return jsonElement.GetString();

                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return default;

                case JsonValueKind.Number:
                    return jsonElement.GetDecimal();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean();

                case JsonValueKind.Object:
                    var enumerateObject = jsonElement.EnumerateObject();
                    var dic = new Dictionary<string, object>();
                    foreach (var item in enumerateObject)
                    {
                        dic.Add(item.Name, ToObject(item.Value));
                    }
                    return dic;

                case JsonValueKind.Array:
                    var enumerateArray = jsonElement.EnumerateArray();
                    var list = new List<object>();
                    foreach (var item in enumerateArray)
                    {
                        list.Add(ToObject(item));
                    }
                    return list;

                default:
                    return default;
            }
        }
    }
}