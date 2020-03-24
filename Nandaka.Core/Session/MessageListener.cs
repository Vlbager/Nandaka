using System;
using System.Threading;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    internal class MessageListener : IDisposable
    {
        private readonly AutoResetEvent _resetEvent;
        private readonly IProtocol _protocol;
        private readonly EventHandler<MessageReceivedEventArgs> _messageReceivedHandler;

        private MessageReceivedEventArgs _lastReceivedMessageArgs;

        public MessageListener(IProtocol protocol)
        {
            _protocol = protocol;
            _resetEvent = new AutoResetEvent(initialState: false);
            _messageReceivedHandler = (sender, args) => OnMessageReceived(args);
            _protocol.MessageReceived += _messageReceivedHandler;
        }

        public bool WaitMessage(TimeSpan waitTimeout, out IFrameworkMessage receivedMessage)
        {
            receivedMessage = default;

            if (!_resetEvent.WaitOne(waitTimeout))
                return false;

            receivedMessage = _lastReceivedMessageArgs.ReceivedMessage;
            return true;
        }

        public void Dispose()
        {
            _protocol.MessageReceived -= _messageReceivedHandler;
            _resetEvent.Dispose();
        }

        private void OnMessageReceived(MessageReceivedEventArgs args)
        {
            _lastReceivedMessageArgs = args;
            _resetEvent.Set();
        }
    }
}