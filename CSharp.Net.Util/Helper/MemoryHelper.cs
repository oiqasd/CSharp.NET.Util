using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace CSharp.Net.Util
{
    /*
     * dumpheap命令:
     * 查看所有释放对象占用内存的统计信息：
     *   dumpheap –type Free -stat   碎片大小/总大小<10% 
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
            string addrs = "0x" + addr.ToString("X");
            //释放
            handle.Free();
            return addrs;
        }

        /// <summary>
        /// Marshal(封送处理)
        /// 主要用于在托管代码（如 C#）和非托管代码（如 C 或 C++）之间进行数据的转换和管理
        /// 可以在 C# 中调用 C 函数时正确地传递数据。
        /// </summary>
        void MarshalDemo()
        {
            int length = 1024;//定义需要申请的内存块大小(1KB)
            IntPtr address = Marshal.AllocHGlobal(length); //从非托管内存中申请内存空间，并返会该内存块的地址 (单位：字节)
            
            Marshal.WriteByte(address, 111);  //修改第一个byte中的数据
            Marshal.WriteByte(address, 0, 111); //修改第一个byte中的数据
            byte buffer0 = Marshal.ReadByte(address); //读取第一个byte中的数据
            byte buffer1 = Marshal.ReadByte(address, 0); //读取第一个byte中的数据

            {
                //source可以是byte[]、也可以是int[]、char[]...
                byte[] source = new byte[] { 1, 2, 3 };
                //将source变量的数组数据拷贝到address内存块中
                Marshal.Copy(source: source,
                    startIndex: 0,          //从source的第一个item开始
                    length: 3,              //选择source的3个item
                    destination: address);  //选择存储的目标 (会写到address内存块的开头处) 
            }
            {
                //dest可以是byte[]、也可以是int[]、char[]...
                byte[] dest = new byte[5];

                Marshal.Copy(source: address,
                    destination: dest,      //#注意：目标数组不能为空、且需要有足够的空间可接收数据
                    startIndex: 1,          //从dest数组的第二个item开始
                    length: 3);             //将address内存块的前3个item写入到dest数组中
            }

            unsafe
            {
                int[] array = new int[5] { 1, 2, 3, 4, 5 };

                int* p = (int*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 1);    //获取数组第二个item的内存地址、并转换成int类型的指针
                char* p2 = (char*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 1); //获取数组第二个item的内存地址、并转换成char类型的指针
            }

            Marshal.FreeHGlobal(address);   //释放非托管内存中分配出的内存 (释放后可立即腾出空间给系统复用)

            {

                string a = "1";
                int n1 = Marshal.SizeOf(a);//返回某个变量在非托管内存中占用的字节数量
                int n2 = Marshal.SizeOf(typeof(Int32));//返回某个类型在非托管内存中占用的字节数量
                IntPtr ma = Marshal.AllocHGlobal(n1);
                Marshal.StructureToPtr(a, ma, false);//将数据从托管对象封送到非托管内存
                string a1 =(string) Marshal.PtrToStructure(ma, typeof(string));//将数据从非托管内存封送到托管对象
                //Marshal.Copy(a.ToCharArray(), 0, ma, a.ToCharArray().Length); // 将托管数组复制到非托管内存
                //byte[] newArray = new byte[4];
                //Marshal.Copy(ma, newArray, 0, newArray.Length);// 将非托管内存指针的数据复制到托管数组  
                Marshal.FreeHGlobal(ma);
            }
            int errorCode = Marshal.GetLastWin32Error();//获取最后一个 Win32 错误代码
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


#if NET
        /// <summary>
        /// 操作共享内存
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <param name="bSize">bytes,default:1024</param>
        [SupportedOSPlatform("windows")]
        public static async void WriteSharedMemory(string name, string data, int bSize = 1024)
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(name, bSize))
            {
                using (var stream = mmf.CreateViewStream())
                {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(data));
                }
            }
        }

        /// <summary>
        /// 读取共享内存
        /// </summary>
        /// <param name="name"></param>
        [SupportedOSPlatform("windows")]
        public static string ReadSharedMemory(string name)
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(name))
            {
                using (var stream = mmf.CreateViewStream())
                {
                    BinaryReader reader = new BinaryReader(stream);
                    return reader.ReadString();
                }
            }
        }
#endif
    }
}
