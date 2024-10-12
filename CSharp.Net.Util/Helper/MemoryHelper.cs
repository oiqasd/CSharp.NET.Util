using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharp.Net.Util
{
    /*
     * dumpheap命令:
     * 查看所有释放对象占用内存的统计信息：dumpheap –type Free -stat   碎片大小/总大小<10%
     * 
     * 
     * **/

    public class MemoryHelper
    {
        /// <summary>
        /// 创建对象防止某个托管对象被垃圾回收或移动，
        /// 通常在非托管代码中需要固定托管对象的内存位置时使用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string Demo(object obj)
        {
            //为对象创建托管对象的句柄,阻止收集托管对象
            //Normal:防止回收,Pinned:固定地址 同fixed,Weak:用于跟踪允许回收,WeakTrackResurrection
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            //获取创建的地址
            //IntPtr addr = GCHandle.ToIntPtr(h);
            IntPtr addr = handle.AddrOfPinnedObject();//获取固定地址
            string addrs= "0x" + addr.ToString("X");
            //释放
            handle.Free();
            return addrs;
        }


        /// <summary>
        /// 获取对象内存地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        unsafe public static string GetReferenceAddress<T>(T obj) where T : class
        {
            if (obj == null)
                return "null";
            // 获取对象的引用地址
            int* ptr = (int*)&obj;
            return $"0x{*(ptr):X}";
        }

    }
}
