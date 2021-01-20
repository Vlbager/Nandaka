using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    internal class SlaveSession : IDisposable
    {
        private readonly NandakaDeviceCtx _deviceCtx;
        private readonly IProtocol _protocol;
        private readonly MessageListener _listener;

        private SlaveSession(NandakaDeviceCtx deviceCtx, IProtocol protocol)
        {
            _deviceCtx = deviceCtx;
            _protocol = protocol;
            _listener = new MessageListener(protocol);
        }

        public static SlaveSession Create(NandakaDeviceCtx deviceCtx, IProtocol protocol)
        {
            return new SlaveSession(deviceCtx, protocol);
        }
        
        public void ProcessNextMessage()
        {
            if (!_listener.WaitMessage(out IMessage? receivedMessage))
                return;
            
            if (receivedMessage!.MessageType != MessageType.Request || receivedMessage.SlaveDeviceAddress != _deviceCtx.Address)
                return;
            
            Log.AppendMessage($"Request message to device-{receivedMessage.SlaveDeviceAddress} received");

            switch (receivedMessage)
            {
                case IRegisterMessage registerMessage:
                    ProcessRegisterMessage(registerMessage);
                    break;
                
                case ISpecificMessage specificMessage:
                    ProcessSpecificMessage(specificMessage);
                    break;
            }
        }

        private void ProcessRegisterMessage(IRegisterMessage registerMessage)
        {
            try
            {
                Log.AppendMessage($"Processing {registerMessage.OperationType.ToString()}-request with {registerMessage.Registers.Count.ToString()} registers");
                
                ProcessRegisterMessageInternal(registerMessage);
                
                Log.AppendMessage("Message processed");
            }
            catch (InvalidMessageReceivedException exception)
            {
                Log.AppendException(exception, "Failed to process message");
                var errorMessage = ErrorMessage.CreateCommon(_deviceCtx.Address, MessageType.Response, exception.ErrorType);
                _protocol.SendMessage(errorMessage);
            }
        }

        private void ProcessRegisterMessageInternal(IRegisterMessage registerMessage)
        {
            IReadOnlyDictionary<IRegister, IRegister> requestMap =
                _deviceCtx.Registers.MapRegistersAsPossible(registerMessage.Registers);
            
            Log.AppendMessage("Updating registers...");

            switch (registerMessage.OperationType)
            {
                case OperationType.Write:
                    requestMap.Update();
                    break;
                
                case OperationType.Read:
                    requestMap.UpdateWithoutValues();
                    break;
                
                default:
                    // todo: create a custom exception
                    throw new Exception("Wrong operation type");
            }

            Log.AppendMessage("Registers updated. Sending response");
            
            var response = new CommonMessage(_deviceCtx.Address, MessageType.Response, registerMessage.OperationType, requestMap.Keys.ToArray());
            _protocol.SendMessage(response);
            
            Log.AppendMessage("Response has been successfully sent");
        }

        private void ProcessSpecificMessage(ISpecificMessage specificMessage)
        {
            Log.AppendMessage("Processing message as specific message");
            
            _deviceCtx.OnSpecificMessageReceived(specificMessage);
            // todo: response on specific message?
            
            Log.AppendWarning("Specific messages response has not been sent. Not Implemented");
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
