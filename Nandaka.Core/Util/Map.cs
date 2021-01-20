using System.Collections;
using System.Collections.Generic;

namespace Nandaka.Core.Util
{
    
    /// <summary>
    /// Thread unsafe mapper.
    /// </summary>
    public sealed class Map<T1, T2> : IEnumerable<KeyValuePair<T1, T2>> 
        where T1: notnull 
        where T2: notnull
    {
        private readonly Dictionary<T1, T2> _forward;
        private readonly Dictionary<T2, T1> _reverse;

        public Indexer<T1, T2> Forward { get; }
        public Indexer<T2, T1> Reverse { get; }

        public Map()
        {
            _forward = new Dictionary<T1, T2>();
            Forward = new Indexer<T1, T2>(_forward);
            _reverse = new Dictionary<T2, T1>();
            Reverse = new Indexer<T2, T1>(_reverse);
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return _forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}