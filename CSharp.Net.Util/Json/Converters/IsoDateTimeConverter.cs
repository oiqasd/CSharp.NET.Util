using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json;

internal sealed class IsoDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default(DateTime);
        if (reader.TokenType == JsonTokenType.Number)
        {
            long value = reader.GetInt64();
            return DateTimeHelper.GetDateTimeFromTimeStamp(value);
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return default(DateTime);

            if (DateTime.TryParse(value, out DateTime d))
                return d;
            if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
                return d;
            if (DateTime.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:sszzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out d))
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
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo));
    }
}

