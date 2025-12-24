using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json.Converters
{
    /// <summary>
    /// 支持将数字转换成字符串
    /// </summary>
    internal sealed class NumberToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    return reader.GetInt64().ToString();
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return reader.GetBoolean().ToString();
                default:
                    return reader.GetString();
            }
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}