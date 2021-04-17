using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    public sealed class MasterSyncSession : IRequestSession<IRegisterMessage, RegisterRequestSentResult>
    {
        private readonly IProtocol _protocol;
        private readonly ForeignDevice _device;
        private readonly DeviceRegistersSynchronizer _synchronizer;

        private ILog Log { get; }

        public MasterSyncSession(IProtocol protocol, ForeignDevice device)
        {
            _protocol = protocol;
            _device = device;
            _synchronizer = new DeviceRegistersSynchronizer(device);
            Log = new PrefixLog(_device.Name);
        }
        
        public IRegisterMessage GetNextMessage()
        {
            return _device.UpdatePolicyField.GetNextMessage(_device);
        }

        public RegisterRequestSentResult SendRequest(IRegisterMessage message)
        {
            Log.AppendMessage($"Sending {message.OperationType.ToString()}-register message");

            _protocol.SendAsPossible(message, out IReadOnlyList<int> requestedRegisterAddresses);

            Log.AppendMessage("Register groups with addresses " +
                              $"{requestedRegisterAddresses.JoinString(", ")} was requested");

            var sentResult = new RegisterRequestSentResult(IsResponseRequired(message), requestedRegisterAddresses);

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
            Log.AppendMessage("Response received, updating registers");
            
            IReadOnlyList<IRegister> updatedRegisters = _synchronizer.UpdateAllRequested(sentResult.RequestedAddresses, response.Registers);

            Log.AppendMessage($"Registers {updatedRegisters.ToLogLine()} updated");
        }

        private void PostProcessRequest(RegisterRequestSentResult sentResult)
        {
            if (sentResult.IsResponseRequired)
                return;
            
            Log.AppendMessage("Set updated state for registers in request");
            
            _synchronizer.MarkAsUpdatedAllRequested(sentResult.RequestedAddresses);
            
            Log.AppendMessage("Requested registers mark as updated");
        }

        private bool IsResponseRequired(IRegisterMessage message)
        {
            if (!_protocol.IsResponseMayBeSkipped)
                return true;

            return message.OperationType == OperationType.Read;
        }
    }
}