using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSharp.Net.Util.Json
{
    internal static class JsonValueExtension
    {
        public static object ToValue(this JsonNode node)
        {
#if NET8_0_OR_GREATER  
            if (node == null) return null;
            switch (node.GetValueKind())
            {
                case JsonValueKind.String:
                    return node.ToString();

                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return default;

                case JsonValueKind.Number:
                    return node.GetValue<decimal>();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return node.GetValue<bool>();

                case JsonValueKind.Object:
                    var enumerateObject = node.AsObject();
                    var dic = new Dictionary<string, object>();
                    foreach (var item in enumerateObject)
                    {
                        dic.Add(item.Key, item.Value.ToValue());
                    }
                    return dic;

                case JsonValueKind.Array:
                    var enumerateArray = node.AsArray();
                    var list = new List<object>();
                    foreach (var item in enumerateArray)
                    {
                        list.Add(item.ToValue());
                    }
                    return list;

                default:
                    return node;
            }
#endif
            return node;
        }
    }
}
