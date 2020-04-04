using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nandaka.Core.Device
{
    public sealed class MasterDevice
    {
        private readonly ObservableCollection<RegisterDevice> _slaveDevices;

        public IReadOnlyCollection<RegisterDevice> SlaveDevices => _slaveDevices;
        public string Name { get; }

        private MasterDevice(IEnumerable<RegisterDevice> slaveDevices, string name)
        {
            Name = name;
            _slaveDevices = new ObservableCollection<RegisterDevice>(slaveDevices);
        }

        public static MasterDevice Start(IEnumerable<RegisterDevice> slaveDevices, string name)
        {
            // todo: start thread logic
            return new MasterDevice(slaveDevices, name);
        }
    }
}
