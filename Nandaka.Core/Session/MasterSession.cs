using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class MasterSession
    {
        private readonly IRegistersUpdatePolicy _registersUpdatePolicy;
        private readonly IDeviceUpdatePolicy _deviceUpdatePolicy;
        private readonly IProtocol _protocol;
        private readonly RegisterDevice _slaveDevice;

        public MasterSession(IProtocol protocol, RegisterDevice slaveDevice, IRegistersUpdatePolicy registersUpdatePolicy,
            IDeviceUpdatePolicy deviceUpdatePolicy)
        {
            _protocol = protocol;
            _slaveDevice = slaveDevice;
            _registersUpdatePolicy = registersUpdatePolicy;
            _deviceUpdatePolicy = deviceUpdatePolicy;
        }

        public void SendNextMessage(TimeSpan waitTime)
        {
            using (var listener = new MessageListener(_protocol))
            {
                IRegisterMessage message = _registersUpdatePolicy.GetNextMessage(_slaveDevice);

                _protocol.SendMessage(message, out IReadOnlyCollection<IRegisterGroup> requestRegisters);

                while (true)
                {
                    if (!listener.WaitMessage(waitTime, out IFrameworkMessage receivedMessage))
                        // todo: create a custom exception
                        throw new Exception("Device Not responding");

                    // All high priority messages should be handled in separate thread.
                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _slaveDevice.Address)
                    {
                        _deviceUpdatePolicy.OnUnexpectedDeviceResponse(_slaveDevice.Address,
                            receivedMessage.SlaveDeviceAddress);
                        continue;
                    }

                    if (!(receivedMessage is IRawRegisterMessage response))
                        // todo: create a custom exception
                        throw new Exception("Wrong response received");

                    UpdateRegisters(response.Registers, requestRegisters, message.OperationType);

                    break;
                }
            }
        }

        public void SendSpecificMessage(ISpecificMessage message, TimeSpan waitTime)
        {
            using (var listener = new MessageListener(_protocol))
            {
                _protocol.SendMessage(message, out _);

                while (true)
                {
                    if (!listener.WaitMessage(waitTime, out IFrameworkMessage receivedMessage))
                        // todo: create a custom exception
                        throw new Exception("Device not responding");

                    if (receivedMessage is HighPriorityMessage)
                        continue;

                    if (receivedMessage.SlaveDeviceAddress != _slaveDevice.Address)
                    {
                        _deviceUpdatePolicy.OnUnexpectedDeviceResponse(_slaveDevice.Address,
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

        private void UpdateRegisters(IReadOnlyCollection<IRegister> responseRegisters,
            IEnumerable<IRegisterGroup> requestGroups, OperationType operationType)
        {
            IReadOnlyDictionary<IRegisterGroup, IRegister[]> registerMap = MapRegisters(requestGroups, responseRegisters);

            if (operationType == OperationType.Write)
                return;
            if (operationType != OperationType.Read)
                // todo: create a custom exception
                throw new Exception("Wrong Operation type");

            foreach (IRegisterGroup registerGroup in registerMap.Keys)
                registerGroup.Update(registerMap[registerGroup]);
        }

        private IReadOnlyDictionary<IRegisterGroup, IRegister[]> MapRegisters(
            IEnumerable<IRegisterGroup> requestGroups,
            IReadOnlyCollection<IRegister> responseRegisters)
        {
            var result = new Dictionary<IRegisterGroup, IRegister[]>();

            try
            {
                foreach (IRegisterGroup requestGroup in requestGroups)
                {
                    IEnumerable<IRegister> registers = Enumerable.Range(requestGroup.Address, requestGroup.Count)
                        .Select(address => responseRegisters.Single(register => register.Address == address));

                    result.Add(requestGroup, registers.ToArray());
                }
            }
            catch (InvalidOperationException exception)
            {
                // todo: create a custom exception
                throw new Exception("Wrong registers received", exception);
            }

            return result;
        }
    }
}
