using System.Collections.Generic;

namespace Nandaka.Core.Helpers
{
    public sealed class Indexer<TIn, TOut> where TIn: notnull
    {
        private readonly Dictionary<TIn, TOut> _dictionary;
            
        public Indexer(Dictionary<TIn, TOut> dictionary)
        {
            _dictionary = dictionary;
        }

        public TOut this[TIn index]
        {
            get => _dictionary[index];
            set => _dictionary[index] = value;
        }

        public bool Contains(TIn index)
        {
            return _dictionary.ContainsKey(index);
        }
    }
}