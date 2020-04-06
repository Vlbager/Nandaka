using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class MasterSession
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

        public void SendNextMessage()
        {
            using (var listener = new MessageListener(_protocol))
            {
                IRegisterMessage message = _registersUpdatePolicy.GetNextMessage(_slaveDevice);
                _log.AppendMessage(LogMessageType.Info, $"Sending {message.OperationType}-register message");

                _protocol.SendMessage(message, out IReadOnlyCollection<IRegisterGroup> requestRegisters);
                _log.AppendMessage(LogMessageType.Info, 
                    $"Register groups with addresses {requestRegisters.GetAllAddressesAsString()} was requested");

                while (true)
                {
                    if (!listener.WaitMessage(_dispatcher.RequestTimeout, out IMessage receivedMessage))
                        // todo: create a custom exception
                        throw new Exception("Device Not responding");

                    // All high priority messages should be handled in separate thread.
                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _slaveDevice.Address)
                    {
                        _dispatcher.OnUnexpectedDeviceResponse(_slaveDevice,
                            receivedMessage.SlaveDeviceAddress);
                        continue;
                    }

                    if (!(receivedMessage is IRawRegisterMessage response))
                        // todo: create a custom exception
                        throw new Exception("Wrong response received");

                    _log.AppendMessage(LogMessageType.Info, "Response received, updating registers");

                    requestRegisters.Update(response.Registers, message.OperationType);

                    _log.AppendMessage(LogMessageType.Info, "Registers updated");

                    break;
                }
            }
        }

        public void SendSpecificMessage(ISpecificMessage message)
        {
            using (var listener = new MessageListener(_protocol))
            {
                _protocol.SendMessage(message, out _);

                while (true)
                {
                    if (!listener.WaitMessage(_dispatcher.RequestTimeout, out IMessage receivedMessage))
                        // todo: create a custom exception
                        throw new Exception("Device not responding");

                    // All high priority messages should be handled in separate thread.
                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _slaveDevice.Address)
                    {
                        _dispatcher.OnUnexpectedDeviceResponse(_slaveDevice,
                            receivedMessage.SlaveDeviceAddress);
                        continue;
                    }

                    if (!(receivedMessage is ISpecificMessage response))
                        // todo: create a custom excepiton
                        throw new Exception("Wrong response received");

                    _slaveDevice.OnSpecificMessageReceived(response);

                    break;
                }
            }
        }
    }
}
