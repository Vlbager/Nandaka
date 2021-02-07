using System;
using Nandaka.Core.Device;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public abstract class ResponseSessionBase<TResponseMessage> : ISession, IDisposable
        where TResponseMessage : IMessage
    {
        private readonly MessageSocket _socket;

        protected abstract ILog Log { get; }

        protected ResponseSessionBase(IProtocol protocol, NandakaDevice device)
        {
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
            Log.AppendMessage(LogLevel.Low, "Waiting for request");

            IMessage message = _socket.WaitMessage();
            
            Log.AppendMessage($"Request message to device-{message.SlaveDeviceAddress} received");
            
            ProcessResponse((TResponseMessage)message);
                
            Log.AppendMessage("Message processed");
        }

        protected abstract void ProcessResponse(TResponseMessage message);

        public void Dispose()
        {
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}