using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSharp.Net.Util.Json;

/// <summary>
/// 字段全小写
/// </summary>
public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public static LowerCaseNamingPolicy Instance { get; } = new LowerCaseNamingPolicy();
    public override string ConvertName(string name) => name.ToLower();
}
