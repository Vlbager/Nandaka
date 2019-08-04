using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public abstract class ProtocolBase<T> : IProtocol
    {
        private readonly IDataPortProvider<T> _dataPortProvider;
        private readonly IComposer<T> _composer;
        private readonly IParser<T> _parser;

        protected ProtocolBase(IDataPortProvider<T> dataPortProvider, IComposer<T> composer, IParser<T> parser)
        {
            _dataPortProvider = dataPortProvider;
            _composer = composer;
            _parser = parser;
            _dataPortProvider.OnDataRecieved += (sender, data) => _parser.Parse(data);
        }

        public void SendMessage(IProtocolMessage message)
        {
            T composedMessage = _composer.Compose(message);
            _dataPortProvider.Write(composedMessage);
        }

        public event EventHandler<IProtocolMessage> MessageRecieved
        {
            add => _parser.MessageParsed += value;
            remove => _parser.MessageParsed += value;
        }
    }
}
