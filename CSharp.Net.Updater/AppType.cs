using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Updater
{
    public enum AppType
    {
        /// <summary>
        /// 控制台程序，自动更新
        /// </summary>
        Console = 1,
        /// <summary>
        /// windows 服务，自动更新
        /// </summary>
        WindowsService = 2,
        /// <summary>
        /// 对话框提示更新，用户选择是否更新
        /// </summary>
        Dialog = 3
    }
}
