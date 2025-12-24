using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json.Converters
{
    internal class BooleanToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString().Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                case JsonTokenType.True:
                    return 1;
                case JsonTokenType.False:
                    return 0;
                case JsonTokenType.Number:
                    return reader.GetInt32();
                default:
                    throw new ArgsException("Invalid Boolean to Int value");
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
