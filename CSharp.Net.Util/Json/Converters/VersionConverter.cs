﻿#if NET
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json
{
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
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
#endif