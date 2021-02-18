using System;
using Nandaka.Core.Device;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public sealed class ResponseSessionHandler<TResponseMessage> : ISessionHandler
        where TResponseMessage : IMessage
    {
        private readonly MessageSocket _socket;
        private readonly IResponseSession<TResponseMessage> _responseSession;
        private readonly ILog _log;

        public ResponseSessionHandler(IResponseSession<TResponseMessage> responseSession, IProtocol protocol, NandakaDevice device)
        {
            _responseSession = responseSession;
            var listener = new MessageListener(protocol);
            
            var filterRules = new MessageFilterRules
            {
                message => message.MessageType == MessageType.Request &&
                           message.SlaveDeviceAddress == device.Address &&
                           message is TResponseMessage
            };
            
            _socket = listener.OpenSocket(filterRules);
            _log = new PrefixLog(device.Name);
        }
        
        public void ProcessNext()
        {
            _log.AppendMessage(LogLevel.Low, "Waiting for request");

            IMessage message = _socket.WaitMessage();
            
            _log.AppendMessage($"Request message to device-{message.SlaveDeviceAddress} received");
            
            _responseSession.ProcessResponse((TResponseMessage)message);
                
            _log.AppendMessage("Message processed");
        }

        public void Dispose()
        {
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}