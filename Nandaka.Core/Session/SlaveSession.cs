using System;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    internal class SlaveSession : IDisposable
    {
        private readonly ForeignDevice _device;
        private readonly IProtocol _protocol;
        private readonly MessageListener _listener;

        private SlaveSession(ForeignDevice device, IProtocol protocol)
        {
            _device = device;
            _protocol = protocol;
            _listener = new MessageListener(protocol);
        }

        public static SlaveSession Create(ForeignDevice device, IProtocol protocol)
        {
            return new SlaveSession(device, protocol);
        }
        
        public void ProcessNextMessage()
        {
            if (!_listener.WaitMessage(out IMessage? receivedMessage))
                return;
            
            if (receivedMessage!.MessageType != MessageType.Request || receivedMessage.SlaveDeviceAddress != _device.Address)
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
            
            Log.AppendMessage("Message processed");
        }

        private void ProcessRegisterMessage(IRegisterMessage registerMessage)
        {
            try
            {
                Log.AppendMessage($"Processing {registerMessage.OperationType.ToString()}-request with {registerMessage.Registers.Count.ToString()} registers");
                
                ProcessRegisterMessageInternal(registerMessage);
            }
            catch (InvalidMessageReceivedException exception)
            {
                Log.AppendException(exception, "Failed to process message");
                var errorMessage = ErrorMessage.CreateCommon(_device.Address, MessageType.Response, exception.ErrorType);
                _protocol.SendMessage(errorMessage);
            }
        }

        private void ProcessRegisterMessageInternal(IRegisterMessage registerMessage)
        {
            var updatePatch = UpdatePatch.GetPatchForPossibleRegisters(_device, registerMessage.Registers);
            
            Log.AppendMessage("Updating registers...");

            updatePatch.Apply();

            Log.AppendMessage("Registers updated. Sending response");
            
            var response = new CommonMessage(_device.Address, MessageType.Response, registerMessage.OperationType, updatePatch.DeviceRegisters);
            _protocol.SendMessage(response);
            
            Log.AppendMessage("Response has been successfully sent");
        }

        private void ProcessSpecificMessage(ISpecificMessage specificMessage)
        {
            Log.AppendMessage("Processing message as specific message");
            
            _device.OnSpecificMessageReceived(specificMessage);
            // todo: response on specific message?
            
            Log.AppendWarning("Specific messages response has not been sent. Not Implemented");
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
