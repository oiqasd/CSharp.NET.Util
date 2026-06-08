/// <summary>
/// 排序参数
/// </summary>
public class SortModel
{
    /// <summary>
    /// 排序字段,必须在<T>中存在，且大小写一致
    /// </summary>
    public string SortName { get; set; }
    /// <summary>
    /// 排序方法 asc:true;desc:false,default: false
    /// </summary>
    public bool Asc { get; set; } = false;
}
