using System;
using System.Collections.Generic;
using Nandaka.Core.Network;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public abstract class ProtocolBase<T> : IProtocol<T>
    {
        private readonly IDataPortProvider<T> _dataPortProvider;
        private readonly IComposer<IRegisterMessage, T> _composer;
        private readonly IParser<T, IRegisterMessage> _parser;

        protected ProtocolBase(IDataPortProvider<T> dataPortProvider, IComposer<IRegisterMessage, T> composer, IParser<T, IRegisterMessage> parser)
        {
            _dataPortProvider = dataPortProvider;
            _composer = composer;
            _parser = parser;
            _dataPortProvider.OnDataReceived += (sender, data) => _parser.Parse(data);
        }

        public abstract IRegisterMessage GetMessage(IEnumerable<IRegisterGroup> registers, int deviceAddress, MessageType type, int errorCode = 0);

        public T PreparePacket(IRegisterMessage message)
        {
            return _composer.Compose(message);
        }

        public void SendPacket(T packet)
        {
            _dataPortProvider.Write(packet);
        }

        public event EventHandler<IRegisterMessage> MessageReceived
        {
            add => _parser.MessageParsed += value;
            remove => _parser.MessageParsed += value;
        }

    }
}
