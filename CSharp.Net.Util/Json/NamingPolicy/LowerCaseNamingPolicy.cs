using System.Text.Json;

namespace CSharp.Net.Util.Json
{
    /// <summary>
    /// 字段全小写
    /// </summary>
    public sealed class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public static LowerCaseNamingPolicy Instance { get; } = new LowerCaseNamingPolicy();
        public override string ConvertName(string name) => name.ToLower();
    }
}