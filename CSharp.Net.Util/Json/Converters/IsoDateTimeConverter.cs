using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json.Converters
{
    internal sealed class IsoDateTimeConverter : JsonConverter<DateTime>
    {
        string format { get; set; } = DateTimeFormatArray.Formats[0];
        public IsoDateTimeConverter()
        {
        }

        public IsoDateTimeConverter(string format)
        {
            this.format = format;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return default;
            if (reader.TokenType == JsonTokenType.Number)
            {
                long value = reader.GetInt64();
                return DateTimeHelper.GetDateTimeFromTimeStamp(value);
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                string value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    return default;

                if (DateTime.TryParse(value, out DateTime d))
                    return d;
                foreach (var f in DateTimeFormatArray.Formats)
                    if (DateTime.TryParseExact(value, f, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
                        return d;

                if ((options.NumberHandling & JsonNumberHandling.AllowReadingFromString) > 0)
                {
                    return DateTimeHelper.GetDateTimeFromTimeStamp(value);
                }
                throw new JsonException($"Could not parse String '{value}' to DateTime.");
            }
            throw new JsonException($"Unexpected JSON token type '{reader.TokenType}' when reading.");
        }
        /// <summary>
        /// 统一输出<![CDATA[yyyy-MM-dd HH:mm:ss]]>格式
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format, DateTimeFormatInfo.InvariantInfo));
        }
    }
}