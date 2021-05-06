using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;

namespace Nandaka.Core.Threading
{
    internal sealed class MasterDeviceSessionMap : IDisposable
    {
        private readonly DeviceUpdatePolicy _updatePolicy;
        private readonly Dictionary<int, DeviceSessionCollection> _sessionsByAddress;

        public MasterDeviceSessionMap(IReadOnlyCollection<DeviceSessionCollection> sessions, DeviceUpdatePolicy updatePolicy)
        {
            _updatePolicy = updatePolicy;
            _sessionsByAddress = sessions.ToDictionary(sessionsByDevice => sessionsByDevice.Device.Address, 
                sessionByDevice => sessionByDevice);
        }

        public DeviceSessionCollection GetNextDevice()
        {
            ForeignDevice device = _updatePolicy.WaitForNextDevice();
            return _sessionsByAddress[device.Address];
        }

        public void Dispose()
        {
            foreach (DeviceSessionCollection deviceSessionCollection in _sessionsByAddress.Values)
                deviceSessionCollection.Dispose();
        }
    }
}