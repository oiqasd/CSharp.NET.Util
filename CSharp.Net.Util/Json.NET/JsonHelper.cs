using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Net.Util.NewtJson
{
    /// <summary>
    /// 使用Newtonsoft.Json序列化
    /// </summary>
    public class JsonHelper
    {
        private static JsonSerializerSettings _jsonSettings;

        static JsonHelper()
        {
            IsoDateTimeConverter datetimeConverter = new IsoDateTimeConverterContent();
            datetimeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            _jsonSettings = new JsonSerializerSettings();
            _jsonSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            _jsonSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            _jsonSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            _jsonSettings.DefaultValueHandling = DefaultValueHandling.Include;//默认值
            //驼峰命名
            //_jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //蛇形命名
            //_jsonSettings.ContractResolver =new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() };
            //小写
            //_jsonSettings.ContractResolver = new LowercaseContractResolver();
            // _jsonSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
            //_jsonSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;

            _jsonSettings.Converters.Add(datetimeConverter);

        }

        /// <summary>
        /// 将指定的对象序列化成 JSON 数据。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns></returns>
        public static string Serialize(object obj, JsonSerializerSettings settings)
        {
            try
            {
                if (null == obj)
                    return null;
#if DEBUG
                return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
#else
                return JsonConvert.SerializeObject(obj, Formatting.None, settings);
#endif
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将指定的对象序列化成 JSON 数据。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="contractResolver">LowercaseContractResolver,CamelCasePropertyNamesContractResolver 等</param>
        /// <param name="includeDefault">default true</param>
        /// <returns></returns>
        public static string Serialize(object obj, IContractResolver contractResolver = null, bool includeDefault = true)
        {
            try
            {
                if (null == obj)
                    return null;

                JsonSerializerSettings setting = _jsonSettings;
                if (contractResolver != null)
                {
                    setting.ContractResolver = contractResolver;
                    if (!includeDefault)
                    {
                        setting.DefaultValueHandling = DefaultValueHandling.Ignore;
                    }
                }
#if DEBUG 
                return JsonConvert.SerializeObject(obj, Formatting.Indented, setting);
#else
                return JsonConvert.SerializeObject(obj, Formatting.None, setting);
#endif
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
                return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
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
                return JsonConvert.DeserializeObject<List<T>>(json, _jsonSettings);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

        /// <summary>
        /// 多个json合并到一个model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="t">实体对象</param>
        /// <returns></returns>
        public static T PopulateObject<T>(string json, T t)
        {
            if (string.IsNullOrEmpty(json))
                return t;
            try
            {
                JsonConvert.PopulateObject(json, t);
                return t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
            var obj = JObject.Parse(jsonData).Properties();
            foreach (JProperty i in obj)
            {
                if (data.ContainsKey(i.Name)) continue;
                data.Add(i.Name, i.Value.ToObject<T>());
            }
            return data;
        }

        /// <summary>
        /// demo 未测试
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private static Dictionary<string, object> RequestApiDic(string jsonData, string field = "")
        {
            string url = string.Empty;
            Dictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                JObject obj = JObject.Parse(jsonData);
                if (obj == null)
                    return data;

                string[] fileds = field.Split(":");
                string tmpVal = jsonData;
                JToken jobj = obj;
                for (int i = 0; i < fileds.Length; i++)
                {
                    if (jobj.Type == JTokenType.Object)
                        jobj = jobj[fileds[i]];
                    else if (jobj.Type == JTokenType.Array)
                    {
                        jobj = jobj.FirstOrDefault();
                        jobj = jobj[fileds[i]];
                    }
                }

                if (jobj == null) return data;

                if (jobj.Type == JTokenType.Array)
                {
                    jobj = jobj.FirstOrDefault();
                    data = GetJObject<object>(jobj.ToString());
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class LowercaseContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}