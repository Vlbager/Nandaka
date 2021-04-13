using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    public sealed class SlaveSyncSession : IResponseSession<IRegisterMessage>
    {
        private readonly IProtocol _protocol;
        private readonly NandakaDevice _device;
        private readonly DeviceRegistersProvider _provider;
        private readonly ILog _log;
        
        public SlaveSyncSession(IProtocol protocol, NandakaDevice device)
        {
            _protocol = protocol;
            _device = device;
            _provider = new DeviceRegistersProvider(device);
            _log = new PrefixLog(device.Name);
        }

        public void ProcessRequest(IRegisterMessage request)
        {
            try
            {
                ProcessRequestInternal(request);
            }
            catch (InvalidMessageReceivedException exception)
            {
                _log.AppendException(exception, "Failed to process message");
                SendErrorResponse(exception);
            }
        }

        private void ProcessRequestInternal(IRegisterMessage request)
        {
            _log.AppendMessage($"Processing {request.OperationType.ToString()}-request with {request.Registers.ToLogLine()} registers");

            switch (request.OperationType)
            {
                case OperationType.Read:
                    ProcessReadRequest(request);
                    break;
                
                case OperationType.Write:
                    ProcessWriteRequest(request);
                    break;
                
                default:
                    throw new InvalidMessageReceivedException($"Invalid operation type received: {request.OperationType.ToString()}",
                                                              ErrorType.InvalidMetaData);
            }
        }

        private void ProcessReadRequest(IRegisterMessage request)
        {
            IReadOnlyList<IRegister> deviceRegisters = _provider.GetDeviceRegisters(request.Registers);

            SendResponseIfRequired(request.OperationType, deviceRegisters);
        }

        private void ProcessWriteRequest(IRegisterMessage message)
        {
            IReadOnlyList<IRegister> updatedDeviceRegisters = _provider.UpdateAllReceived(message.Registers);

            SendResponseIfRequired(message.OperationType, updatedDeviceRegisters);
        }

        private void SendResponseIfRequired(OperationType operationType, IReadOnlyList<IRegister> registers)
        {
            if (_protocol.IsResponseMayBeSkipped && operationType == OperationType.Write)
            {
                _log.AppendMessage("Write message response will be skipped");
                return;
            }
            
            var response = new CommonMessage(_device.Address, MessageType.Response, operationType, registers);
            _protocol.SendMessage(response);

            _log.AppendMessage("Response has been successfully sent");
        }

        private void SendErrorResponse(InvalidMessageReceivedException exception)
        {
            var errorMessage = ErrorMessage.CreateCommon(_device.Address, MessageType.Response, exception.ErrorType);
            _protocol.SendMessage(errorMessage);
        }
    }
}