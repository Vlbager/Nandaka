using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Device
{
    public sealed class MasterDevice : IDevice
    {
        private readonly ObservableCollection<SlaveDevice> _slaveDevices;

        public IProtocol Protocol { get; }
        public IReadOnlyCollection<SlaveDevice> SlaveDevices => _slaveDevices;

        public MasterDevice(IProtocol protocol)
        {
            Protocol = protocol;
            _slaveDevices = new ObservableCollection<SlaveDevice>();
        }

        public void AddSlaveDevice(SlaveDevice device)
        {
            if (device.Protocol.GetType() != Protocol.GetType())
                // todo: create a custom exception
                throw new Exception();

            _slaveDevices.Add(device);
        }

        public void RemoveSlaveDevice(SlaveDevice device)
        {
            if (_slaveDevices.Contains(device))
                _slaveDevices.Remove(device);
        }
    }
}
