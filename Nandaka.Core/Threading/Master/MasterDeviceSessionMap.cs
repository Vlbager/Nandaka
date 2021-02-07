using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;

namespace Nandaka.Core.Threading
{
    internal sealed class MasterDeviceSessionMap
    {
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly Dictionary<int, DeviceSessionCollection> _sessionsByAddress;

        public MasterDeviceSessionMap(IReadOnlyCollection<DeviceSessionCollection> sessions, MasterDeviceDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _sessionsByAddress = sessions.ToDictionary(sessionsByDevice => sessionsByDevice.Device.Address, 
                sessionByDevice => sessionByDevice);
        }

        public DeviceSessionCollection GetNextDevice()
        {
            ForeignDevice device = _dispatcher.GetNextDevice();
            return _sessionsByAddress[device.Address];
        }
    }
}