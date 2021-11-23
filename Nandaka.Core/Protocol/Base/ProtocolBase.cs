using System;
using System.Collections.Generic;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Network;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public abstract class ProtocolBase<T> : IProtocol
    {
        private readonly IDataPortProvider<T> _dataPortProvider;
        private readonly IComposer<IMessage, T> _composer;
        private readonly IParser<T, MessageReceivedEventArgs> _parser;
        
        public abstract IProtocolInfo Info { get; }
        public abstract bool IsResponseMayBeSkipped { get; }
        public abstract bool IsAsyncRequestsAllowed { get; }

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
            _dataPortProvider.OnDataReceived += (_, data) => _parser.Parse(data);
        }
        
        public SentMessageResult SendMessage(IMessage message)
        {
            T packet = _composer.Compose(message);
            _dataPortProvider.Write(packet);
            
            if (message is IRegisterMessage registerMessage)
                return SentMessageResult.CreateSuccessResult(registerMessage.Registers);
            
            return SentMessageResult.CreateSuccessResult();
        }
    }
}
