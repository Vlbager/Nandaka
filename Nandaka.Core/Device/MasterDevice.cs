using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nandaka.Core.Device
{
    public sealed class MasterDevice
    {
        private readonly ObservableCollection<RegisterDevice> _slaveDevices;

        public IReadOnlyCollection<RegisterDevice> SlaveDevices => _slaveDevices;

        private MasterDevice(IEnumerable<RegisterDevice> slaveDevices)
        {
            _slaveDevices = new ObservableCollection<RegisterDevice>(slaveDevices);
        }

        public static MasterDevice Start(IReadOnlyCollection<RegisterDevice> slaveDevices)
        {
            // todo: start thread logic
            return new MasterDevice(slaveDevices);
        }
    }
}
