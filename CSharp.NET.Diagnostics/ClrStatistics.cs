using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CSharp.NET.Diagnostics
{
    public class ClrStatistics
    {
        public static void Dump()
        {
            int processId = Process.GetCurrentProcess().Id;
            using (DataTarget dataTarget = DataTarget.CreateSnapshotAndAttach(processId))
            {

            }
        }
        public static string GetHeap(ulong filterSize = 0)
        {
            int processId = Process.GetCurrentProcess().Id;
        
            // 创建数据源对象
            DataTarget dataTarget = null;
            try
            {
                dataTarget = DataTarget.AttachToProcess(processId,false);
                StringBuilder sb = new StringBuilder();
                // 获取CLR版本
                ClrInfo clr = dataTarget.ClrVersions[0];
                Dictionary<ulong, (int Count, ulong Size, string Name)> stats
                    = new Dictionary<ulong, (int Count, ulong Size, string Name)>();

                using (ClrRuntime runtime = clr.CreateRuntime())
                {
                    ClrHeap heap = runtime.Heap;
                    if (heap == null || !heap.CanWalkHeap) return "";
                    sb.AppendLine(heap.ToString());
                    //Console.WriteLine("{0,16} {1,16} {2,8} {3}", "Object", "MethodTable", "Size", "Type");
                    foreach (ClrObject obj in heap.EnumerateObjects())
                    {
                        //Console.WriteLine($"{obj.Address:x16} {obj.Type?.MethodTable:x16} {obj.Size,8:D} {obj.Type.Name}");
                        if (obj.Type?.MethodTable is null)
                            continue;

                        if (!stats.TryGetValue(obj.Type.MethodTable, out (int Count, ulong Size, string Name) item))
                            item = (0, 0, obj.Type.Name);

                        stats[obj.Type.MethodTable] = (item.Count + 1, item.Size + obj.Size, item.Name);
                    }

                    sb.AppendLine("\nStatistics:");
                    var sorted = from i in stats
                                 where i.Value.Size >= filterSize
                                 orderby i.Value.Size descending
                                 select new
                                 {
                                     i.Key,
                                     i.Value.Name,
                                     i.Value.Size,
                                     i.Value.Count
                                 };

                    sb.AppendLine(string.Format("{0,16} {1,12} {2,12}\t{3}", "MethodTable", "Count", "Size", "Type"));
                    foreach (var item in sorted)
                        sb.AppendLine($"{item.Key:x16} {item.Count,12:D} {item.Size,12:D}\t{item.Name}");

                    sb.AppendLine($"Total {sorted.Sum(x => x.Count):0} objects");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                dataTarget?.Dispose();
                dataTarget = null;
            }
        }
    }
}
