using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 排序参数
/// </summary>
public class SortModel
{
    /// <summary>
    /// 排序字段
    /// </summary>
    public string ColumnName { get; set; }
    /// <summary>
    /// 排序方法 asc:true;desc:false
    /// </summary>
    public bool SortAsc { get; set; }
}
