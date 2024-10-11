using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharp.Net.Util
{
    public class MemoryHelper
    {
        /// <summary>
        /// 获取引用对象内存地址
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetObjectAddress(object o)
        {
            GCHandle h = GCHandle.Alloc(o, GCHandleType.WeakTrackResurrection);
            IntPtr addr = GCHandle.ToIntPtr(h);
            return "0x" + addr.ToString("X");
        }

        /*
        /// <summary>
        /// 获取对象内存地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static unsafe string GetReferenceAddress<T>(T obj) where T : class
        {
            if (obj == null)
                return "null";
            // 获取对象的引用地址
            int* ptr = (int*)&obj;
            return $"0x{*(ptr):X}";
        }
        */
    }
}
