using System;
using System.Collections.Generic;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Network;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public abstract class ProtocolBase<T> : IProtocol
    {
        private readonly IDataPortProvider<T> _dataPortProvider;
        private readonly IComposer<IMessage, T> _composer;
        private readonly IParser<T, MessageReceivedEventArgs> _parser;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived
        {
            add => _parser.MessageParsed += value;
            remove => _parser.MessageParsed -= value;
        }

        protected ProtocolBase(IDataPortProvider<T> dataPortProvider, IComposer<IMessage, T> composer, IParser<T, MessageReceivedEventArgs> parser)
        {
            _dataPortProvider = dataPortProvider;
            _composer = composer;
            _parser = parser;
            _dataPortProvider.OnDataReceived += (sender, data) => _parser.Parse(data);
        }

        public void SendAsPossible(IRegisterMessage message, out IReadOnlyList<IRegister> sentRegisters)
        {
            T packet = _composer.Compose(message, out sentRegisters);
            _dataPortProvider.Write(packet);
        }

        public void SendMessage(IMessage message)
        {
            T packet = _composer.Compose(message, out IReadOnlyList<IRegister> sentRegister);
            if (message is IRegisterMessage registerMessage && registerMessage.Registers.Count != sentRegister.Count)
                throw new TooMuchDataRequestedException("Can't send all registers");
            
            _dataPortProvider.Write(packet);
        }
    }
}
