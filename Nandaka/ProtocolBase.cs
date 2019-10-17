using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public abstract class ProtocolBase<T> : IProtocol<T>
    {
        private readonly IDataPortProvider<T> _dataPortProvider;
        private readonly IComposer<IMessage, T> _composer;
        private readonly IParser<T, IMessage> _parser;

        protected ProtocolBase(IDataPortProvider<T> dataPortProvider, IComposer<IMessage, T> composer, IParser<T, IMessage> parser)
        {
            _dataPortProvider = dataPortProvider;
            _composer = composer;
            _parser = parser;
            _dataPortProvider.OnDataRecieved += (sender, data) => _parser.Parse(data);
        }

        public abstract IMessage GetMessage(IEnumerable<IRegister> registers, int deviceAddress, MessageType type, int errorCode = 0);

        public T PreparePacket(IMessage message)
        {
            return _composer.Compose(message);
        }

        public void SendPacket(T packet)
        {
            _dataPortProvider.Write(packet);
        }

        public event EventHandler<IMessage> MessageReceived
        {
            add => _parser.MessageParsed += value;
            remove => _parser.MessageParsed += value;
        }

    }
}
