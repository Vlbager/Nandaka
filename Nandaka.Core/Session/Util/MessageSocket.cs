using System;
using System.Collections.Concurrent;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    internal sealed class MessageSocket : IDisposable
    {
        private readonly IProtocol _protocol;
        private readonly EventHandler<MessageReceivedEventArgs> _messageReceivedHandler;
        private readonly BlockingCollection<MessageReceivedEventArgs> _receivedMessages;

        public MessageSocket(IProtocol protocol, MessageFilterRules filterRules)
        {
            _protocol = protocol;
            _receivedMessages = new BlockingCollection<MessageReceivedEventArgs>();
            _messageReceivedHandler = (_, args) => OnMessageReceived(args, filterRules);
            _protocol.MessageReceived += _messageReceivedHandler;
        }
        
        public bool WaitMessage(out IMessage? receivedMessage, TimeSpan waitTimeout)
        {
            receivedMessage = default;
            
            if (!_receivedMessages.TryTake(out MessageReceivedEventArgs? receivedEventArgs, waitTimeout))
                return false;

            receivedMessage = receivedEventArgs!.ReceivedMessage;

            return false;
        }

        public IMessage WaitMessage()
        {
            return _receivedMessages.Take().ReceivedMessage;
        }

        public void Dispose()
        {
            _protocol.MessageReceived -= _messageReceivedHandler;
            _receivedMessages.Dispose();
        }
        
        private void OnMessageReceived(MessageReceivedEventArgs receivedMessageArgs, MessageFilterRules filterRules)
        {
            if (receivedMessageArgs.IsException() || filterRules.CheckMessage(receivedMessageArgs.ReceivedMessage))
                _receivedMessages.TryAdd(receivedMessageArgs);
        }
    }
}