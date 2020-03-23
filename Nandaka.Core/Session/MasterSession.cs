using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class MasterSession
    {
        private readonly IRegistersUpdatePolicy _updatePolicy;

        public IProtocol Protocol { get; }
        public RegisterDevice SlaveDevice { get; }

        public MasterSession(IProtocol protocol, RegisterDevice slaveDevice, IRegistersUpdatePolicy updatePolicy)
        {
            Protocol = protocol;
            SlaveDevice = slaveDevice;
            _updatePolicy = updatePolicy;
        }

        public void SendRegisterMessage(out IReadOnlyCollection<IRegisterGroup> sentGroups)
        {
            IRegisterMessage message = _updatePolicy.GetNextMessage(SlaveDevice);

            Protocol.SendMessage(message, out sentGroups);
        }
    }
}
