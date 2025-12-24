using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util.LiteSockets
{
    /// <summary>
    /// 文件分块传输
    /// </summary>
    public class FileChunk
    {
        public string TransferId { get; set; } = Guid.NewGuid().ToString(); // 每个文件唯一传输ID
        public string FileName { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
