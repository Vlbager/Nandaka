using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    internal class MasterSession
    {
        private readonly ILog _log;
        private readonly IRegistersUpdatePolicy _registersUpdatePolicy;
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly IProtocol _protocol;
        private readonly ForeignDeviceCtx _foreignDeviceCtx;

        public MasterSession(IProtocol protocol, ForeignDeviceCtx foreignDeviceCtx, MasterDeviceDispatcher dispatcher, ILog log)
        {
            _log = new PrefixLog(log, $"[{foreignDeviceCtx.Name} Session]");
            _protocol = protocol;
            _foreignDeviceCtx = foreignDeviceCtx;
            _registersUpdatePolicy = foreignDeviceCtx.UpdatePolicy;
            _dispatcher = dispatcher;
        }

        public void ProcessNextMessage()
        {
            using (var listener = new MessageListener(_protocol))
            {
                IRegisterMessage message = _registersUpdatePolicy.GetNextMessage(_foreignDeviceCtx);

                if (message is EmptyMessage)
                {
                    _log.AppendMessage(LogMessageType.Info, $"Nothing to process. Skip {_foreignDeviceCtx.Name}");
                    return;
                }
                
                _log.AppendMessage(LogMessageType.Info, $"Sending {message.OperationType}-register message");

                _protocol.SendAsPossible(message, out IReadOnlyList<IRegister> requestRegisters);
                _log.AppendMessage(LogMessageType.Info, 
                    $"Register groups with addresses {requestRegisters.GetAllAddressesAsString()} was requested");

                while (true)
                {
                    if (!listener.WaitMessage(_dispatcher.RequestTimeout, out IMessage? receivedMessage))
                        throw new DeviceNotRespondException("Device Not responding");

                    if (receivedMessage!.MessageType != MessageType.Response)
                        continue;
                    
                    // All high priority messages should be handled in separate thread.
                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _foreignDeviceCtx.Address)
                    {
                        _dispatcher.OnUnexpectedDeviceResponse(_foreignDeviceCtx,
                            receivedMessage.SlaveDeviceAddress);
                        continue;
                    }
                    
                    if (receivedMessage is ErrorMessage errorMessage)
                        ProcessErrorMessage(errorMessage);

                    if (!(receivedMessage is IRegisterMessage response))
                        throw new InvalidMetaDataReceivedException("Wrong response received");

                    _log.AppendMessage(LogMessageType.Info, "Response received, updating registers");

                    UpdateRegisters(requestRegisters, response.Registers, response.OperationType);

                    _log.AppendMessage(LogMessageType.Info, "Registers updated");

                    break;
                }
            }
        }

        public void ProcessSpecificMessage(ISpecificMessage message)
        {
            using var listener = new MessageListener(_protocol);
            
            _protocol.SendMessage(message);

            while (true)
            {
                if (!listener.WaitMessage(_dispatcher.RequestTimeout, out IMessage? receivedMessage))
                    throw new DeviceNotRespondException("Device Not responding");
                    
                if (receivedMessage!.MessageType != MessageType.Response)
                    continue;

                // All high priority messages should be handled in separate thread.
                if (receivedMessage is HighPriorityMessage)
                    continue;

                if (receivedMessage.SlaveDeviceAddress != _foreignDeviceCtx.Address)
                {
                    _dispatcher.OnUnexpectedDeviceResponse(_foreignDeviceCtx,
                                                           receivedMessage.SlaveDeviceAddress);
                    continue;
                }
                    
                if (receivedMessage is ErrorMessage errorMessage)
                    ProcessErrorMessage(errorMessage);

                if (!(receivedMessage is ISpecificMessage response))
                    throw new InvalidMetaDataReceivedException("Wrong response received");

                _foreignDeviceCtx.OnSpecificMessageReceived(response);

                break;
            }
        }

        private void ProcessErrorMessage(ErrorMessage errorMessage)
        {
            throw new NotImplementedException();
        }

        private static void UpdateRegisters(IReadOnlyCollection<IRegister> registersToUpdate, IReadOnlyCollection<IRegister> sourceRegisters,
            OperationType operationType)
        {
            IReadOnlyDictionary<IRegister, IRegister> registerMap = registersToUpdate.MapRegistersToAllGroups(sourceRegisters);

            switch (operationType)
            {
                case OperationType.Read:
                    registerMap.Update();
                    break;
                
                case OperationType.Write:
                    registerMap.UpdateWithoutValues();
                    break;
                
                default:
                    throw new NandakaBaseException("Wrong operation type");
            }
        }
    }
}
