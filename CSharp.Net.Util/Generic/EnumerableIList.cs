using System.Collections;
using System.Collections.Generic;

namespace CSharp.Net.Util
{
    public static class EnumerableIList
    {
        public static EnumerableIList<T> Create<T>(IList<T> list)
        {
            return new EnumerableIList<T>(list);
        }
    }

    public struct EnumerableIList<T> : IEnumerableIList<T>, IEnumerable<T>, IEnumerable, IList<T>, ICollection<T>
    {
        private readonly IList<T> _list;

        public static EnumerableIList<T> Empty;

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public EnumerableIList(IList<T> list)
        {
            _list = list;
        }

        public EnumeratorIList<T> GetEnumerator()
        {
            return new EnumeratorIList<T>(_list);
        }

        public static implicit operator EnumerableIList<T>(List<T> list)
        {
            return new EnumerableIList<T>(list);
        }

        public static implicit operator EnumerableIList<T>(T[] array)
        {
            return new EnumerableIList<T>(array);
        }

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
