using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Session
{
    public sealed class SlaveSyncSession : IResponseSession<IRegisterMessage>
    {
        private readonly IProtocol _protocol;
        private readonly NandakaDevice _device;
        private readonly DeviceRegistersSynchronizer _synchronizer;
        private readonly ILogger _logger;
        
        public SlaveSyncSession(IProtocol protocol, NandakaDevice device, ILogger logger)
        {
            _protocol = protocol;
            _device = device;
            _synchronizer = new DeviceRegistersSynchronizer(device);
            _logger = logger;
        }

        public void ProcessRequest(IRegisterMessage request)
        {
            try
            {
                ProcessRequestInternal(request);
            }
            catch (InvalidMessageReceivedException exception)
            {
                _logger.LogError(exception, "Failed to process message");
                SendErrorResponse(exception);
            }
        }

        private void ProcessRequestInternal(IRegisterMessage request)
        {
            _logger.LogInformation("Processing request {0}", request);

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
            IReadOnlyList<IRegister> deviceRegisters = _synchronizer.GetDeviceRegisters(request.Registers);
            _synchronizer.MarkAsUpdatedAllRequested(request.Registers);

            SendResponse(request.OperationType, deviceRegisters);
        }

        private void ProcessWriteRequest(IRegisterMessage message)
        {
            IReadOnlyList<IRegister> updatedDeviceRegisters = _synchronizer.UpdateAllReceived(message.Registers);
            
            if (_protocol.IsResponseMayBeSkipped)
            {
                _logger.LogInformation("Write message response will be skipped");
                return;
            }

            SendResponse(message.OperationType, updatedDeviceRegisters);
        }

        private void SendResponse(OperationType operationType, IReadOnlyList<IRegister> registers)
        {
            var response = new CommonMessage(_device.Address, MessageType.Response, operationType, registers);
            _protocol.SendMessage(response);

            _logger.LogInformation("Response has been successfully sent");
        }

        private void SendErrorResponse(InvalidMessageReceivedException exception)
        {
            var errorMessage = ErrorMessage.CreateCommon(_device.Address, MessageType.Response, exception.ErrorType);
            _protocol.SendMessage(errorMessage);
        }
    }
}