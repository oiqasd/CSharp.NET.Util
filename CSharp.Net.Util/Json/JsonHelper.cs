using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Standard.Util
{
    /// <summary>
    /// 使用System.Text.Json序列化
    /// Newtonsoft.Json 迁移文档
    /// https://docs.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to?pivots=dotnet-5-0
    /// </summary>
    public class JsonHelper
    {
        static JsonSerializerOptions serializeOptions;

        static JsonHelper()
        {
            serializeOptions = new JsonSerializerOptions
            {
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,//使用 camel 大小写 
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,//允许或写入带引号的数字,例如:"23"
                //ReferenceHandler = ReferenceHandler.Preserve,//保留引用并处理循环引用
                //WriteIndented = true,//对JSON输出进行优质打印
                DefaultIgnoreCondition=JsonIgnoreCondition.WhenWritingNull,//等同NullValueHandling.Ignore

            };
            serializeOptions.Converters.Add(new IsoDateTimeOffsetConverter());
            serializeOptions.Converters.Add(new VersionConverter());
        }


        /// <summary>
        /// 将指定的对象序列化成 JSON 数据
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            try
            {
                if (null == obj)
                    return null;

                return JsonSerializer.Serialize(obj, serializeOptions);
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
                return JsonSerializer.Deserialize<T>(json, serializeOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }




    /// <summary>
    /// 内置支持的唯一格式是 ISO 8601-1:2019
    /// 2020-11-11T21:08:18
    /// 所以需要重写
    /// </summary>
    internal sealed class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    /// <summary>
    /// .NET 7 允许使用空格的 Version 类型添加自定义转换器
    /// </summary>
    internal sealed class VersionConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? versionString = reader.GetString();
            if (Version.TryParse(versionString, out Version? result))
            {
                return result;
            }

            ThrowHelper.ThrowJsonException();
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
