using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Standard.Util
{
    /// <summary>
    /// 枚举条目信息
    /// </summary>
    public sealed class EnumItem
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public EnumItem(int value, string name, string description)
        {
            Value = value;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 枚举值
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 枚举说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 枚举名
        /// </summary>
        public string Name { get; set; }
    }
}