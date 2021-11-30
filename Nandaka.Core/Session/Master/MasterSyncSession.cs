using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Util;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Session
{
    public sealed class MasterSyncSession : IRequestSession<IRegisterMessage, RegisterRequestSentResult>
    {
        private readonly IProtocol _protocol;
        private readonly ForeignDevice _device;
        private readonly DeviceRegistersSynchronizer _synchronizer;
        private readonly ILogger _logger;

        public MasterSyncSession(IProtocol protocol, ForeignDevice device, ILogger logger)
        {
            _protocol = protocol;
            _device = device;
            _logger = logger;
            _synchronizer = new DeviceRegistersSynchronizer(device);
        }
        
        public IRegisterMessage GetNextMessage()
        {
            return _device.UpdatePolicyField.GetNextMessage(_device);
        }

        public RegisterRequestSentResult SendRequest(IRegisterMessage message)
        {
            _logger.LogDebug("Sending message: {0}", message);

            SentMessageResult result = _protocol.SendMessage(message);
            
            _logger.LogDebug("Requested registers: {0}", new RegistersLogMessage(result.SentRegisters));

            var sentResult = new RegisterRequestSentResult(IsResponseRequired(message), result.SentRegisters);

            PostProcessRequest(sentResult);

            return sentResult;
        }

        public void ProcessResponse(IMessage message, RegisterRequestSentResult sentResult)
        {
            if (message is not IRegisterMessage registerMessage)
                throw new NandakaBaseException($"Unexpected message type: {message.GetType()}");
            
            ProcessRegisterMessageResponse(registerMessage, sentResult);
        }

        private void ProcessRegisterMessageResponse(IRegisterMessage response, RegisterRequestSentResult sentResult)
        {
            _logger.LogDebug("Response received, updating registers");
            
            IReadOnlyList<IRegister> updatedRegisters = _synchronizer.UpdateAllRequested(sentResult.RequestedRegisters, response.Registers);

            _logger.LogDebug($"Registers {updatedRegisters.ToLogLine()} updated");
        }

        private void PostProcessRequest(RegisterRequestSentResult sentResult)
        {
            if (sentResult.IsResponseRequired)
                return;
            
            _logger.LogDebug("Set updated state for registers in request");
            
            _synchronizer.MarkAsUpdatedAllRequested(sentResult.RequestedRegisters);
            
            _logger.LogDebug("Requested registers mark as updated");
        }

        private bool IsResponseRequired(IRegisterMessage message)
        {
            if (!_protocol.IsResponseMayBeSkipped)
                return true;

            return message.OperationType == OperationType.Read;
        }
    }
}