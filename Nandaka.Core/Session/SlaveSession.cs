using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class SlaveSession : IDisposable
    {
        private readonly NandakaDevice _device;
        private readonly IProtocol _protocol;
        private readonly ILog _log;
        private readonly MessageListener _listener;

        private SlaveSession(NandakaDevice device, IProtocol protocol, ILog log)
        {
            _device = device;
            _protocol = protocol;
            _log = log;
            _listener = new MessageListener(protocol);
        }

        public static SlaveSession Create(NandakaDevice device, IProtocol protocol, ILog log)
        {
            var sessionLog = new PrefixLog(log, "[Session]");
            return new SlaveSession(device, protocol, sessionLog);
        }
        
        public void ListenNextMessage()
        {
            _listener.WaitMessage(out IMessage receivedMessage);
            
            //todo: processing messages + logger

            if (receivedMessage is IRawRegisterMessage registerMessage)
            {
                
            }
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
