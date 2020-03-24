using System;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly IFrameworkMessage _receivedMessage;

        public IFrameworkMessage ReceivedMessage 
        {
            get
            {
                if (_exception != null)
                    throw _exception;

                return _receivedMessage;
            }
        }

        public MessageReceivedEventArgs(IFrameworkMessage receivedMessage)
        {
            _receivedMessage = receivedMessage;
        }

        public MessageReceivedEventArgs(Exception exception)
        {
            _exception = exception;
        }
    }
}