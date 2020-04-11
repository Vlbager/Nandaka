using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class SlaveSession : IDisposable
    {
        private readonly NandakaDevice _device;
        private readonly IProtocol _protocol;
        private readonly ILog _log;
        private readonly MessageListener _listener;

        private SlaveSession(NandakaDevice device, IProtocol protocol, ILog log)
        {
            _device = device;
            _protocol = protocol;
            _log = log;
            _listener = new MessageListener(protocol);
        }

        public static SlaveSession Create(NandakaDevice device, IProtocol protocol, ILog log)
        {
            var sessionLog = new PrefixLog(log, "[Session]");
            return new SlaveSession(device, protocol, sessionLog);
        }
        
        public void ProcessNextMessage()
        {
            // todo: fix sometimes returning null in received message.
            if (!_listener.WaitMessage(out IMessage receivedMessage))
                return;
            
            // todo: logger
            if (receivedMessage.Type != MessageType.Request || receivedMessage.SlaveDeviceAddress != _device.Address)
                return;

            switch (receivedMessage)
            {
                case IReceivedMessage registerMessage:
                    ProcessRegisterMessage(registerMessage);
                    break;
                
                case ISpecificMessage specificMessage:
                    ProcessSpecificMessage(specificMessage);
                    break;
            }
        }

        private void ProcessRegisterMessage(IReceivedMessage registerMessage)
        {
            try
            {
                ProcessRegisterMessageInternal(registerMessage);
            }
            // todo: exception handling system
            catch (Exception exception)
            {
                var errorMessage = new CommonErrorMessage(_device.Address, MessageType.Response, ErrorType.InvalidRegisterAddress);
                _protocol.SendMessage(errorMessage);
            }
        }

        private void ProcessRegisterMessageInternal(IReceivedMessage registerMessage)
        {
            IReadOnlyDictionary<IRegisterGroup, IRegister[]> requestMap =
                _device.RegisterGroups.MapRegistersToPossibleGroups(registerMessage.Registers);

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

            var response = new CommonMessage(_device.Address, MessageType.Response, registerMessage.OperationType, requestMap.Keys);
            _protocol.SendMessage(response);
        }

        private void ProcessSpecificMessage(ISpecificMessage specificMessage)
        {
            _device.OnSpecificMessageReceived(specificMessage);
            // todo: response on specific message?
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
