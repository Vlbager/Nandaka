using System;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly Exception? _exception;
        private readonly IMessage? _receivedMessage;

        public IMessage ReceivedMessage 
        {
            get
            {
                if (_exception != null)
                    throw _exception;

                return _receivedMessage!;
            }
        }

        public MessageReceivedEventArgs(IMessage receivedMessage)
        {
            _receivedMessage = receivedMessage;
        }

        public MessageReceivedEventArgs(Exception exception)
        {
            _exception = exception;
        }
    }
}