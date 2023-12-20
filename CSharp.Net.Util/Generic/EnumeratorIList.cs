using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Net.Util
{
    public struct EnumeratorIList<T> : IEnumerator<T>, IEnumerator, IDisposable
    {
        private readonly IList<T> _list;

        private int _index;

        public T Current => _list[_index];

        object? IEnumerator.Current => Current;

        public EnumeratorIList(IList<T> list)
        {
            _index = -1;
            _list = list;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < (_list?.Count ?? 0);
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
