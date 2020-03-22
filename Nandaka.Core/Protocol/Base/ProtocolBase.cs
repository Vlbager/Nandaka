using System;
using System.Collections.Generic;
using Nandaka.Core.Network;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public abstract class ProtocolBase<T> : IProtocol
    {
        private readonly IDataPortProvider<T> _dataPortProvider;
        private readonly IComposer<IFrameworkMessage, T> _composer;
        private readonly IParser<T, IFrameworkMessage> _parser;

        protected ProtocolBase(IDataPortProvider<T> dataPortProvider, IComposer<IFrameworkMessage, T> composer, IParser<T, IFrameworkMessage> parser)
        {
            _dataPortProvider = dataPortProvider;
            _composer = composer;
            _parser = parser;
            _dataPortProvider.OnDataReceived += (sender, data) => _parser.Parse(data);
        }

        public void SendMessage(IFrameworkMessage message, out IReadOnlyCollection<IRegisterGroup> sentGroups)
        {
            T packet = _composer.Compose(message, out sentGroups);
            _dataPortProvider.Write(packet);
        }

        public event EventHandler<IFrameworkMessage> MessageReceived
        {
            add => _parser.MessageParsed += value;
            remove => _parser.MessageParsed -= value;
        }
    }
}
