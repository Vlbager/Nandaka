using System;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{

    internal sealed class MessageListener : IDisposable
    {
        private readonly IProtocol _protocol;

        public MessageListener(IProtocol protocol)
        {
            _protocol = protocol;
        }

        /// <summary>
        /// Open socket and start listening the protocol message thread.
        /// </summary>
        public MessageSocket OpenSocket(MessageFilterRules filterRules)
        {
            //todo:NAN-22
            return new MessageSocket(_protocol, filterRules);
        }

        public void Dispose()
        {
            //todo:NAN-22
        }
    }
}