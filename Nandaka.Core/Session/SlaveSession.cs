﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    internal class SlaveSession : IDisposable
    {
        private readonly ForeignDeviceCtx _deviceCtx;
        private readonly IProtocol _protocol;
        private readonly ILog _log;
        private readonly MessageListener _listener;

        private SlaveSession(ForeignDeviceCtx deviceCtx, IProtocol protocol, ILog log)
        {
            _deviceCtx = deviceCtx;
            _protocol = protocol;
            _log = log;
            _listener = new MessageListener(protocol);
        }

        public static SlaveSession Create(ForeignDeviceCtx deviceCtx, IProtocol protocol, ILog log)
        {
            var sessionLog = new PrefixLog(log, "[Session]");
            return new SlaveSession(deviceCtx, protocol, sessionLog);
        }
        
        public void ProcessNextMessage()
        {
            if (!_listener.WaitMessage(out IMessage? receivedMessage))
                return;
            
            if (receivedMessage!.MessageType != MessageType.Request || receivedMessage.SlaveDeviceAddress != _deviceCtx.Address)
                return;
            
            _log.AppendMessage(LogMessageType.Info, $"Request message to device-{receivedMessage.SlaveDeviceAddress} received");

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
                _log.AppendMessage(LogMessageType.Info,$"Processing {registerMessage.OperationType}-request with {registerMessage.Registers.Count} registers");
                
                ProcessRegisterMessageInternal(registerMessage);
                
                _log.AppendMessage(LogMessageType.Info, "Message processed");
            }
            catch (InvalidMessageReceivedException exception)
            {
                _log.AppendMessage(LogMessageType.Warning, exception.ToString());
                var errorMessage = ErrorMessage.CreateCommon(_deviceCtx.Address, MessageType.Response, exception.ErrorType);
                _protocol.SendMessage(errorMessage);
            }
        }

        private void ProcessRegisterMessageInternal(IRegisterMessage registerMessage)
        {
            IReadOnlyDictionary<IRegister, IRegister> requestMap =
                _deviceCtx.Registers.MapRegistersAsPossible(registerMessage.Registers);
            
            _log.AppendMessage(LogMessageType.Info, "Updating registers...");

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

            _log.AppendMessage(LogMessageType.Info, "Registers updated. Sending response");
            
            var response = new CommonMessage(_deviceCtx.Address, MessageType.Response, registerMessage.OperationType, requestMap.Keys.ToArray());
            _protocol.SendMessage(response);
            
            _log.AppendMessage(LogMessageType.Info, "Response has been successfully sent");
        }

        private void ProcessSpecificMessage(ISpecificMessage specificMessage)
        {
            _log.AppendMessage(LogMessageType.Info,"Processing message as specific message");
            
            _deviceCtx.OnSpecificMessageReceived(specificMessage);
            // todo: response on specific message?
            
            _log.AppendMessage(LogMessageType.Warning, "Specific messages response has not been sent. Not Implemented");
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
