using System;
using System.Collections.Concurrent;
using System.Threading;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    internal class MessageListener : IDisposable
    {
        private readonly AutoResetEvent _resetEvent;
        private readonly IProtocol _protocol;
        private readonly EventHandler<MessageReceivedEventArgs> _messageReceivedHandler;
        private readonly ConcurrentQueue<MessageReceivedEventArgs> _receivedMessages;

        public MessageListener(IProtocol protocol)
        {
            _protocol = protocol;
            _resetEvent = new AutoResetEvent(initialState: false);
            _receivedMessages = new ConcurrentQueue<MessageReceivedEventArgs>();
            _messageReceivedHandler = (sender, args) => OnMessageReceived(args);
            _protocol.MessageReceived += _messageReceivedHandler;
        }

        public bool WaitMessage(TimeSpan waitTimeout, out IMessage receivedMessage)
        {
            receivedMessage = default;

            if (_receivedMessages.IsEmpty && !_resetEvent.WaitOne(waitTimeout))
                return false;

            _receivedMessages.TryDequeue(out MessageReceivedEventArgs receivedEventArgs);
            receivedMessage = receivedEventArgs.ReceivedMessage;
            
            return true;
        }

        public bool WaitMessage(out IMessage receivedMessage)
        {
            receivedMessage = default;

            if (_receivedMessages.IsEmpty && !_resetEvent.WaitOne())
                return false;

            _receivedMessages.TryDequeue(out MessageReceivedEventArgs receivedEventArgs);
            if (receivedEventArgs == null)
                return false;
            
            receivedMessage = receivedEventArgs.ReceivedMessage;
            
            return true;
        }

        public void Dispose()
        {
            _protocol.MessageReceived -= _messageReceivedHandler;
            _resetEvent.Dispose();
        }

        private void OnMessageReceived(MessageReceivedEventArgs receivedMessageArgs)
        {
            _receivedMessages.Enqueue(receivedMessageArgs);
            _resetEvent.Set();
        }
    }
}