using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nandaka.Core.Device
{
    public sealed class MasterDeviceManager
    {
        private readonly ObservableCollection<NandakaDevice> _slaveDevices;

        public IReadOnlyCollection<NandakaDevice> SlaveDevices => _slaveDevices;
        public string Name { get; }

        private MasterDeviceManager(IEnumerable<NandakaDevice> slaveDevices, string name)
        {
            Name = name;
            _slaveDevices = new ObservableCollection<NandakaDevice>(slaveDevices);
        }

        public static MasterDeviceManager Start(IEnumerable<NandakaDevice> slaveDevices, string name)
        {
            // todo: start thread logic
            return new MasterDeviceManager(slaveDevices, name);
        }
    }
}
