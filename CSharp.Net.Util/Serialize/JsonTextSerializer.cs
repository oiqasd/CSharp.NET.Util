using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CSharp.Net.Util.Serialize
{
    /// <summary>
    /// 基于 System.Text.Json 的通用序列化器。
    /// </summary>
    public sealed class JsonTextSerializer : ISerializer
    {
        public string EncodName { get; set; } = "utf-8";

        public T Deserialize<T>(string source) where T : class
        {
            if (string.IsNullOrEmpty(source))
                return default(T);
            return JsonSerializer.Deserialize<T>(source);
        }

        public T Deserialize<T>(Stream source) where T : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return JsonSerializer.DeserializeAsync<T>(source).GetAwaiter().GetResult();
        }

        public T Deserialize<T>(byte[] source) where T : class
        {
            if (source == null || source.Length == 0)
                return default(T);
            return JsonSerializer.Deserialize<T>(source);
        }

        public T DeserializeFromFile<T>(string fileName) where T : class
        {
            if (!File.Exists(fileName))
                return default(T);
            using (var stream = File.OpenRead(fileName))
                return Deserialize<T>(stream);
        }

        public string Serialize<T>(T value) where T : class =>
            JsonSerializer.Serialize(value);

        public void Serialize<T>(T value, Stream destination) where T : class
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            JsonSerializer.SerializeAsync(destination, value).GetAwaiter().GetResult();
        }

        public void Serialize<T>(T value, ref byte[] buffer) where T : class =>
            buffer = JsonSerializer.SerializeToUtf8Bytes(value);

        public void Serialize<T>(T value, string fileName) where T : class
        {
            using (var stream = File.Create(fileName))
                Serialize(value, stream);
        }
    }
}
