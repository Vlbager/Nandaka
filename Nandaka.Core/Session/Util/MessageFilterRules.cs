using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Session
{
    public sealed class MessageFilterRules : IReadOnlyCollection<Predicate<IMessage>>
    {
        private readonly List<Predicate<IMessage>> _rules;

        public int Count => _rules.Count;

        public MessageFilterRules()
        {
            _rules = new List<Predicate<IMessage>>();
        }

        public void Add(Predicate<IMessage> rule) => _rules.Add(rule);

        public bool CheckMessage(IMessage message) => _rules.All(rule => rule(message));

        public IEnumerator<Predicate<IMessage>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}