using System;

namespace CSharp.Net.Util
{
    /// <summary>
    /// 运算工具类
    /// </summary>
    public class Operators
    {
        /// <summary>
        /// 生成运算码
        /// </summary>
        /// <param name="index">基码: 0~31 0: 返回0,代表没有意义</param>
        /// <returns></returns>
        public static int BitsCode(byte index)
        {
            if (index < 0 || index > 31)
                throw new ArgumentOutOfRangeException("arg range 0~31.");
            return (int)BitwiseCode(index);
        }

        /// <summary>
        /// 生成位与运算码
        /// </summary>
        /// <param name="move">基码: 0~255，同一组不能重复;要返回Int,则不能超过31. 0: 返回0,代表没有意义</param>
        /// <returns>输入0则返回0,其它返回1的左移 <![CDATA[1<<move-1]]> 次</returns> 
        static uint BitwiseCode(byte move)
        {
            if (move == 0) return 0;
            return (uint)1 << move - 1;
            //return Math.Pow(2, target - 1);
        }

        /// <summary>
        /// 计算位或数值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index">基码,0:代表没有意义</param>
        /// <returns>source | <![CDATA[1<<index-1]]></returns>
        public static uint BitsValue(uint source, byte index)
        {
            if (index == 0) return source;
            return source | BitwiseCode(index);
        }

        /// <summary>
        /// 比较位与后是否相等
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index">基码,0:代表没有意义</param>
        /// <returns>true,false</returns>
        public static bool BitEqual(uint source, byte index)
        {
            var t = BitwiseCode(index);
            return (source & t) == t;
        }

    }
}
