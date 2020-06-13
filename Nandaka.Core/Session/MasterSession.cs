using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    internal class MasterSession
    {
        private readonly ILog _log;
        private readonly IRegistersUpdatePolicy _registersUpdatePolicy;
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly IProtocol _protocol;
        private readonly NandakaDevice _slaveDevice;

        public MasterSession(IProtocol protocol, NandakaDevice slaveDevice, MasterDeviceDispatcher dispatcher, ILog log)
        {
            _log = new PrefixLog(log, $"[{slaveDevice.Name} Session]");
            _protocol = protocol;
            _slaveDevice = slaveDevice;
            _registersUpdatePolicy = slaveDevice.UpdatePolicy;
            _dispatcher = dispatcher;
        }

        public void ProcessNextMessage()
        {
            using (var listener = new MessageListener(_protocol))
            {
                IRegisterMessage message = _registersUpdatePolicy.GetNextMessage(_slaveDevice);

                if (message is EmptyMessage)
                {
                    _log.AppendMessage(LogMessageType.Info, $"Nothing to process. Skip {_slaveDevice.Name}");
                    return;
                }
                
                _log.AppendMessage(LogMessageType.Info, $"Sending {message.OperationType}-register message");

                _protocol.SendAsPossible(message, out IReadOnlyCollection<IRegisterGroup> requestRegisters);
                _log.AppendMessage(LogMessageType.Info, 
                    $"Register groups with addresses {requestRegisters.GetAllAddressesAsString()} was requested");

                while (true)
                {
                    if (!listener.WaitMessage(_dispatcher.RequestTimeout, out IMessage receivedMessage))
                        throw new DeviceNotRespondException("Device Not responding");

                    if (receivedMessage.Type != MessageType.Response)
                        continue;
                    
                    // All high priority messages should be handled in separate thread.
                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _slaveDevice.Address)
                    {
                        _dispatcher.OnUnexpectedDeviceResponse(_slaveDevice,
                            receivedMessage.SlaveDeviceAddress);
                        continue;
                    }
                    
                    if (receivedMessage is IErrorMessage errorMessage)
                        ProcessErrorMessage(errorMessage);

                    if (!(receivedMessage is IReceivedMessage response))
                        throw new InvalidMetaDataException("Wrong response received");

                    _log.AppendMessage(LogMessageType.Info, "Response received, updating registers");

                    UpdateRegisters(requestRegisters, response.Registers, response.OperationType);

                    _log.AppendMessage(LogMessageType.Info, "Registers updated");

                    break;
                }
            }
        }

        public void ProcessSpecificMessage(ISpecificMessage message)
        {
            using (var listener = new MessageListener(_protocol))
            {
                _protocol.SendMessage(message);

                while (true)
                {
                    if (!listener.WaitMessage(_dispatcher.RequestTimeout, out IMessage receivedMessage))
                        throw new DeviceNotRespondException("Device Not responding");
                    
                    if (receivedMessage.Type != MessageType.Response)
                        continue;

                    // All high priority messages should be handled in separate thread.
                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _slaveDevice.Address)
                    {
                        _dispatcher.OnUnexpectedDeviceResponse(_slaveDevice,
                            receivedMessage.SlaveDeviceAddress);
                        continue;
                    }
                    
                    if (receivedMessage is IErrorMessage errorMessage)
                        ProcessErrorMessage(errorMessage);

                    if (!(receivedMessage is ISpecificMessage response))
                        throw new InvalidMetaDataException("Wrong response received");

                    _slaveDevice.OnSpecificMessageReceived(response);

                    break;
                }
            }
        }

        private void ProcessErrorMessage(IErrorMessage errorMessage)
        {
            throw new NotImplementedException();
        }

        private static void UpdateRegisters(IReadOnlyCollection<IRegisterGroup> groupsToUpdate, IReadOnlyCollection<IRegister> sourceRegisters,
            OperationType operationType)
        {
            IReadOnlyDictionary<IRegisterGroup, IRegister[]> registerMap = groupsToUpdate.MapRegistersToAllGroups(sourceRegisters);

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
