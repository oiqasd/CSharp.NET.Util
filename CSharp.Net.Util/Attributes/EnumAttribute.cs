using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 枚举排序
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class EnumOrderAttribute : Attribute
    {
        /// <summary>
        /// 枚举的排序值
        /// </summary>
        public int EnumOrder { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="order"></param>
        public EnumOrderAttribute(int order)
        {
            EnumOrder = order;
        }

        /// <summary>
        /// 通过枚举类型和枚举返回对应的序号
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="enumValue">枚举</param>
        /// <returns></returns>
        public static int GetOrder(Type type, object enumValue)
        {
            object[] objs = type.GetField(enumValue.ToString()).GetCustomAttributes(typeof(EnumOrderAttribute), false);
            if (objs.Length > 0)
            {
                var attribute = objs[0] as EnumOrderAttribute;
                if (attribute != null) return attribute.EnumOrder;
            }
            return 0;
        }
    }

    /// <summary>
    /// 英文描述特性
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class EDescriptionAttribute : Attribute
    {
        /// <summary>
        /// 英文描述
        /// </summary>
        public string EDescription { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="order"></param>
        public EDescriptionAttribute(string eDescription)
        {
            EDescription = eDescription;
        }
    }

}