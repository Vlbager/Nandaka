using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Device
{
    public sealed class MasterDevice
    {
        private readonly ObservableCollection<RegisterDevice> _slaveDevices;

        public IProtocol Protocol { get; }
        public IReadOnlyCollection<RegisterDevice> SlaveDevices => _slaveDevices;

        private MasterDevice(IProtocol protocol, IEnumerable<RegisterDevice> slaveDevices)
        {
            Protocol = protocol;
            _slaveDevices = new ObservableCollection<RegisterDevice>(slaveDevices);
        }

        public static MasterDevice Create(IProtocol protocol, IReadOnlyCollection<RegisterDevice> slaveDevices)
        {
            if (slaveDevices.Any(device => device.Protocol.GetType() != protocol.GetType()))
                // todo: create a custom exception
                throw new Exception();

            return new MasterDevice(protocol, slaveDevices);
        }
    }
}
