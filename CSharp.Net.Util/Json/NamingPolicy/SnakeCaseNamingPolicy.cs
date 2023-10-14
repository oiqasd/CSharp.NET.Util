using System.Linq;
using System.Text.Json;

namespace CSharp.Net.Util.Json
{
    /// <summary>
    /// 字段蛇形命名
    /// </summary>
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();
        public override string ConvertName(string name)
            => string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
}