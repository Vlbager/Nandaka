using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public sealed class ResponseSessionHandler<TResponseMessage> : ISessionHandler
        where TResponseMessage : IMessage
    {
        private readonly MessageSocket _socket;
        private readonly IResponseSession<TResponseMessage> _responseSession;
        private readonly ILogger _logger;

        public ResponseSessionHandler(IResponseSession<TResponseMessage> responseSession, IProtocol protocol, NandakaDevice device, ILogger logger)
        {
            _responseSession = responseSession;
            _logger = logger;
            var listener = new MessageListener(protocol);
            
            var filterRules = new MessageFilterRules
            {
                message => message.MessageType == MessageType.Request &&
                           message.SlaveDeviceAddress == device.Address &&
                           message is TResponseMessage
            };
            
            _socket = listener.OpenSocket(filterRules);
        }
        
        public void ProcessNext()
        {
            _logger.LogDebug("Waiting for request");

            IMessage message = _socket.WaitMessage();
            
            _logger.LogDebug("Message received: {0}", message);
            
            _responseSession.ProcessRequest((TResponseMessage)message);
                
            _logger.LogDebug("Message processed");
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}