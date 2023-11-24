using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json.Converters
{
    internal class StringToLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default(long);

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt64();

            if (reader.TokenType == JsonTokenType.String)
                return ConvertHelper.ConvertTo(reader.GetString(), 0L);

            return default(long);
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
