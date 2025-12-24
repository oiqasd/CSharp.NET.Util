using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json.Converters
{
    internal sealed class EmptyToNullDateTimeConverter : JsonConverter<DateTime?>
    {
        string format { get; set; } = DateTimeFormatArray.Formats[0];
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            if (reader.TokenType == JsonTokenType.Number)
            {
                long value = reader.GetInt64();
                return DateTimeHelper.GetDateTimeFromTimeStamp(value);
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString();

                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                if (DateTime.TryParse(value, out DateTime dateTime))
                {
                    return dateTime;
                }

                foreach (var f in DateTimeFormatArray.Formats)
                    if (DateTime.TryParseExact(value, f, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out dateTime))
                        return dateTime;

                if ((options.NumberHandling & JsonNumberHandling.AllowReadingFromString) > 0)
                {
                    return DateTimeHelper.GetDateTimeFromTimeStamp(value);
                }
            }
            // 如果不是字符串类型或解析失败，尝试使用默认解析
            return JsonSerializer.Deserialize<DateTime?>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(format, DateTimeFormatInfo.InvariantInfo));
            }
            else
            {
                writer.WriteStringValue("");
            }
        }
    }
}
