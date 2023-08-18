using System.Collections.Generic;

namespace CSharp.Net.Mvc;

/// <summary>
/// 接口列表
/// </summary>
public class ApiInfo
{
    /// <summary>
    /// 控制器代码
    /// </summary>
    public string ControllerCode { get; set; }
    /// <summary>
    /// 控制器描述
    /// </summary>
    public string ControllerDescribe { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }
    /// <summary>
    /// 接口列表
    /// </summary>
    public List<ActionInfo> ActionList { get; set; }
}

/// <summary>
/// action信息
/// </summary>
public class ActionInfo
{
    /// <summary>
    /// 接口代码
    /// </summary>
    public string ActionCode { get; set; }
    /// <summary>
    /// 接口描述
    /// </summary>
    public string ActionDescribe { get; set; }
}
