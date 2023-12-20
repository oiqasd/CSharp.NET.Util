using System.Collections;
using System.Collections.Generic;

namespace CSharp.Net.Util
{
    internal interface IEnumerableIList<T> : IEnumerable<T>, IEnumerable
    {
        new EnumeratorIList<T> GetEnumerator();
    }
}
