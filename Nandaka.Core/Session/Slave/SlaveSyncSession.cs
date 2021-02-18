using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public sealed class SlaveSyncSession : IResponseSession<IRegisterMessage>
    {
        private readonly IProtocol _protocol;
        private readonly NandakaDevice _device;
        private readonly ILog _log;
        
        public SlaveSyncSession(IProtocol protocol, NandakaDevice device)
        {
            _protocol = protocol;
            _device = device;
            _log = new PrefixLog(device.Name);
        }

        public void ProcessResponse(IRegisterMessage message)
        {
            if (_protocol.IsResponseMayBeSkipped && message.OperationType == OperationType.Write)
            {
                _log.AppendMessage("Write message response will be skipped");
                return;
            }
            
            try
            {
                ProcessResponseInternal(message);
            }
            catch (InvalidMessageReceivedException exception)
            {
                _log.AppendException(exception, "Failed to process message");
                var errorMessage = ErrorMessage.CreateCommon(_device.Address, MessageType.Response, exception.ErrorType);
                _protocol.SendMessage(errorMessage);
            }
        }

        private void ProcessResponseInternal(IRegisterMessage message)
        {
            _log.AppendMessage($"Processing {message.OperationType.ToString()}-request with {message.Registers.Count.ToString()} registers");
            
            var updatePatch = UpdatePatch.CreatePatchForPossibleRegisters(_device, message.Registers);
            
            _log.AppendMessage("Updating registers...");

            updatePatch.Apply();

            _log.AppendMessage("Registers updated. Sending response");
            
            var response = new CommonMessage(_device.Address, MessageType.Response, message.OperationType, updatePatch.DeviceRegisters);
            _protocol.SendMessage(response);
            
            _log.AppendMessage("Response has been successfully sent");
        }
    }
}