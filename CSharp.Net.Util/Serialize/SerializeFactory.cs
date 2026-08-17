using System;

namespace CSharp.Net.Util.Serialize
{
    /// <summary>
    /// 序列化器工厂。默认使用 JSON。
    /// </summary>
    public static class SerializeFactory
    {
        public static ISerializer GetSerializer(string mode = null)
        {
            switch ((mode ?? "json").Trim().ToLowerInvariant())
            {
                case "":
                case "json":
                    return new JsonTextSerializer();
                case "xml":
                    return CreateXmlSerializer();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Supported serializers are json and xml.");
            }
        }

        public static ISerializer GetSerializer(SerializeMethod mode)
        {
            switch (mode)
            {
                case SerializeMethod.Json:
                    return new JsonTextSerializer();
                case SerializeMethod.Xml:
                    return CreateXmlSerializer();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported serializer.");
            }
        }

        private static ISerializer CreateXmlSerializer() =>
            new XMLSerializer { EncodName = "utf-8" };
    }
}
