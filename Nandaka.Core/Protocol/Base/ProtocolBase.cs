using System;
using Nandaka.Core.Network;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public abstract class ProtocolBase<T> : IProtocol<T>
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

        public T PreparePacket(IFrameworkMessage message)
        {
            return _composer.Compose(message);
        }

        public void SendPacket(T packet)
        {
            _dataPortProvider.Write(packet);
        }

        public event EventHandler<IFrameworkMessage> MessageReceived
        {
            add => _parser.MessageParsed += value;
            remove => _parser.MessageParsed -= value;
        }

    }
}
