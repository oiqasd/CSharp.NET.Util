using System.Threading;

namespace CSharp.Net.Util.LiteSockets
{
    public class FileTransferContext
    {
        public string TransferId { get; }
        public string FileName { get; }
        public long TotalBytes { get; }
        public long TransferredBytes { get; set; }
        public bool IsCanceled { get; private set; }
        private readonly CancellationTokenSource _cts = new();

        public FileTransferContext(string id, string fileName, long totalBytes)
        {
            TransferId = id;
            FileName = fileName;
            TransferredBytes = totalBytes;
        }

        public CancellationToken Token => _cts.Token;
        public void Cancel() => _cts.Cancel();

    }
}
