namespace CSharp.Net.Mvc;

/// <summary>
/// 用于反射接口特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class RefClassAttribute : Attribute
{
    public RefClassAttribute()
    {
        Sort = 999;
    }
    public RefClassAttribute(string name, int sort)
    {
        Sort = sort;
        Name = name;
    }

    public int Sort { get; set; }

    public string Name { get; set; }
}
