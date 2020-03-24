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
        private readonly IRegistersUpdatePolicy _updatePolicy;
        private readonly IProtocol _protocol;

        public RegisterDevice SlaveDevice { get; }

        public MasterSession(IProtocol protocol, RegisterDevice slaveDevice, IRegistersUpdatePolicy updatePolicy)
        {
            _protocol = protocol;
            SlaveDevice = slaveDevice;
            _updatePolicy = updatePolicy;
        }

        public void SendNextMessage(TimeSpan waitTime)
        {
            using (var listener = new MessageListener(_protocol))
            {
                IRegisterMessage message = _updatePolicy.GetNextMessage(SlaveDevice);

                _protocol.SendMessage(message, out IReadOnlyCollection<IRegisterGroup> requestRegisters);

                do
                {
                    if (!listener.WaitMessage(waitTime, out IFrameworkMessage frameworkMessage))
                        // todo: create a custom exception
                        throw new Exception("Device Not responding");

                    if (frameworkMessage is IHighPriorityMessage highPriorityMessage)
                        // note: may be do nothing? All high priority messages can be handled in separate thread.
                        DoSomethingWithHighPriorityMessage(highPriorityMessage);

                    if (frameworkMessage.SlaveDeviceAddress != SlaveDevice.Address)
                        // note: need to notice master thread about this, but continue listen next messages
                        throw new NotImplementedException();

                    if (!(frameworkMessage is IRawRegisterMessage response))
                        // todo: create a custom exception
                        throw new Exception("Wrong response received");

                    UpdateDeviceRegisters(response.Registers, requestRegisters, message.OperationType);

                } while (false);
            }
        }

        private void DoSomethingWithHighPriorityMessage(IHighPriorityMessage message)
        {
            throw new NotImplementedException();
        }


        private void UpdateDeviceRegisters(IReadOnlyCollection<IRegister> responseRegisters,
            IReadOnlyCollection<IRegisterGroup> requestGroups, OperationType operationType)
        {
            AssertRegistersAddresses(responseRegisters, requestGroups);

            if (operationType == OperationType.Write)
                return;

            foreach (IRegisterGroup registerGroup in requestGroups)
            {
                //todo: update device
            }
        }

        private void AssertRegistersAddresses(IReadOnlyCollection<IRegister> registers,
            IReadOnlyCollection<IRegisterGroup> groups)
        {
            IEnumerable<IRegister> groupRegisters = groups.SelectMany(group => group.GetRawRegisters());

            int matchedPairsCount = registers.Join(groupRegisters,
                    register => register.Address,
                    groupRegister => groupRegister.Address,
                    (register, groupRegister) => 1)
                .Sum();

            if (registers.Count != matchedPairsCount)
                throw new Exception("Received registers has invalid count");
        }
    }
}
