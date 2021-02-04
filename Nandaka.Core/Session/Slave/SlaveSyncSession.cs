using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public sealed class SlaveSyncSession : ResponseSessionBase<IRegisterMessage>
    {
        private readonly IProtocol _protocol;
        private readonly NandakaDevice _device;
        
        protected override ILog Log { get; }
        
        public SlaveSyncSession(IProtocol protocol, NandakaDevice device) : base(protocol, device)
        {
            _protocol = protocol;
            _device = device;
            Log = new PrefixLog(device.Name);
        }

        protected override void ProcessResponse(IRegisterMessage message)
        {
            if (_protocol.IsResponseMayBeSkipped && message.OperationType == OperationType.Write)
            {
                Log.AppendMessage("Write message response will be skipped");
                return;
            }
            
            try
            {
                ProcessResponseInternal(message);
            }
            catch (InvalidMessageReceivedException exception)
            {
                Log.AppendException(exception, "Failed to process message");
                var errorMessage = ErrorMessage.CreateCommon(_device.Address, MessageType.Response, exception.ErrorType);
                _protocol.SendMessage(errorMessage);
            }
        }

        private void ProcessResponseInternal(IRegisterMessage message)
        {
            Log.AppendMessage($"Processing {message.OperationType.ToString()}-request with {message.Registers.Count.ToString()} registers");
            
            var updatePatch = UpdatePatch.CreatePatchForPossibleRegisters(_device, message.Registers);
            
            Log.AppendMessage("Updating registers...");

            updatePatch.Apply();

            Log.AppendMessage("Registers updated. Sending response");
            
            var response = new CommonMessage(_device.Address, MessageType.Response, message.OperationType, updatePatch.DeviceRegisters);
            _protocol.SendMessage(response);
            
            Log.AppendMessage("Response has been successfully sent");
        }
    }
}