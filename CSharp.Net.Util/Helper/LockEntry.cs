using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    public static class LockEntry
    {
        private static readonly ConcurrentDictionary<string, Entry> Locks =
            new ConcurrentDictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        public static async Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A lock key is required.", nameof(key));

            while (true)
            {
                Entry entry = Locks.GetOrAdd(key, _ => new Entry());
                lock (entry.SyncRoot)
                {
                    if (entry.Removed)
                        continue;
                    entry.ReferenceCount++;
                }

                try
                {
                    await entry.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    return new Lease(key, entry);
                }
                catch
                {
                    ReleaseReference(key, entry, false);
                    throw;
                }
            }
        }

        private static void ReleaseReference(string key, Entry entry, bool releaseSemaphore)
        {
            if (releaseSemaphore)
                entry.Semaphore.Release();

            bool dispose = false;
            lock (entry.SyncRoot)
            {
                entry.ReferenceCount--;
                if (entry.ReferenceCount == 0)
                {
                    entry.Removed = true;
                    Locks.TryRemove(key, out Entry removed);
                    dispose = ReferenceEquals(removed, entry);
                }
            }

            if (dispose)
                entry.Semaphore.Dispose();
        }

        private sealed class Entry
        {
            public readonly object SyncRoot = new object();
            public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
            public int ReferenceCount;
            public bool Removed;
        }

        private sealed class Lease : IDisposable
        {
            private readonly string _key;
            private Entry _entry;

            public Lease(string key, Entry entry)
            {
                _key = key;
                _entry = entry;
            }

            public void Dispose()
            {
                Entry entry = Interlocked.Exchange(ref _entry, null);
                if (entry != null)
                    ReleaseReference(_key, entry, true);
            }
        }
    }
}
