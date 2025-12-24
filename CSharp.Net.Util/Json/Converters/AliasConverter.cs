using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharp.Net.Util.Json.Converters
{
    /// <summary>
    /// 反序列化自定义别名
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class JsonDeserializationAliasAttribute : Attribute
    {
        public string Alias { get; }

        public JsonDeserializationAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    /// <summary>
    /// 反序列化自定义别名
    /// 默认还是使用JsonPropertyName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AliasConverter<T> : JsonConverter<T> where T : new()
    {
        // 缓存属性及其别名的映射关系
        private readonly Dictionary<string, PropertyInfo> _propertyMappings;
        public AliasConverter()
        {
            // 初始化属性映射
            _propertyMappings = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(
                    prop => GetJsonPropertyName(prop),
                    prop => prop
                );

            // 添加别名映射
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var jsonPropertyName = GetJsonPropertyName(prop);
                var aliases = prop.GetCustomAttributes<JsonDeserializationAliasAttribute>()
                    .Select(a => a.Alias)
                    .ToList();

                foreach (var alias in aliases)
                {
                    if (!_propertyMappings.ContainsKey(alias))
                    {
                        _propertyMappings.Add(alias, prop);
                    }
                }
            }

        }
        /// <summary>
        ///  获取属性的序列化名称（JsonPropertyName特性指定的值或属性名）
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private string GetJsonPropertyName(PropertyInfo prop)
        {
            var jsonPropAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            return jsonPropAttr?.Name ?? prop.Name;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var result = new T();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return result;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    // 查找匹配的属性（支持别名）
                    if (_propertyMappings.TryGetValue(propertyName, out var propertyInfo))
                    {
                        // 反序列化属性值
                        var value = JsonSerializer.Deserialize(ref reader, propertyInfo.PropertyType, options);
                        propertyInfo.SetValue(result, value);
                    }
                    else
                    {
                        // 跳过未知属性
                        reader.Skip();
                    }
                }
            }

            throw new JsonException("Unexpected end of JSON");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // 序列化时使用默认行为，保持原有名称
            writer.WriteStartObject();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyName = GetJsonPropertyName(prop);
                var propertyValue = prop.GetValue(value);

                if (propertyValue != null)
                {
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, propertyValue, prop.PropertyType, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
