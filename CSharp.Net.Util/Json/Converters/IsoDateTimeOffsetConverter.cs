//#if NET

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json
{
    /// <summary>
    /// 内置支持的唯一格式是 ISO 8601-1:2019
    /// 2020-11-11T21:08:18 需要重写
    /// </summary>
    internal sealed class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            if (DateTimeOffset.TryParse(value, out DateTimeOffset d))
                return d;
            if (DateTimeOffset.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
                return d;
            if (DateTimeOffset.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:sszzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
                return d;
            throw new JsonException($"Could not parse String '{value}' to DateTimeOffset.");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo));
        }
    }
}
//#endif
