using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public sealed class MasterSyncSession : RequestSessionBase<IRegisterMessage, RegisterRequestSentResult>
    {
        private readonly IProtocol _protocol;
        private readonly ForeignDevice _device;
        
        protected override ILog Log { get; }

        public MasterSyncSession(IProtocol protocol, TimeSpan requestTimeout, ForeignDevice device) 
            : base(protocol, device, requestTimeout)
        {
            _protocol = protocol;
            _device = device;
            Log = new PrefixLog(_device.Name);
        }
        
        protected override IRegisterMessage GetNextMessage()
        {
            return _device.UpdatePolicy.GetNextMessage(_device);
        }

        protected override RegisterRequestSentResult SendRequest(IRegisterMessage message)
        {
            Log.AppendMessage($"Sending {message.OperationType.ToString()}-register message");

            _protocol.SendAsPossible(message, out IReadOnlyList<int> requestedRegisterAddresses);

            Log.AppendMessage("Register groups with addresses " +
                              $"{requestedRegisterAddresses.Select(a => a.ToString()).Join(", ")} was requested");

            return new RegisterRequestSentResult(IsResponseRequired(message), requestedRegisterAddresses);
        }

        protected override void ProcessResponse(IMessage message, RegisterRequestSentResult sentResult)
        {
            switch (message)
            {
                case IRegisterMessage registerResponse:
                    ProcessRegisterMessageResponse(registerResponse, sentResult);
                    break;
                
                case ErrorMessage errorResponse:
                    ProcessErrorResponse(errorResponse);
                    break;
                
                default:
                    throw new NandakaBaseException($"Unexpected message type: {message.GetType()}");
            }
        }

        private void ProcessErrorResponse(ErrorMessage errorMessage)
        {
            throw new NotImplementedException();
        }

        private void ProcessRegisterMessageResponse(IRegisterMessage response, RegisterRequestSentResult sentResult)
        {
            Log.AppendMessage("Response received, updating registers");

            var patch = UpdatePatch.CreatePatchForAllRegisters(_device, sentResult.RequestedAddresses, response.Registers);
            patch.Apply();

            Log.AppendMessage("Registers updated");
        }

        private bool IsResponseRequired(IRegisterMessage message)
        {
            if (!_protocol.IsResponseMayBeSkipped)
                return true;

            return message.OperationType == OperationType.Read;
        }
    }
}