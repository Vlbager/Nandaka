using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;

namespace Nandaka.Core.Threading
{
    internal sealed class MasterDeviceSessionMap : IDisposable
    {
        private readonly DeviceUpdatePolicyWrapper _updatePolicyWrapper;
        private readonly Dictionary<int, DeviceSessionCollection> _sessionsByAddress;

        public MasterDeviceSessionMap(IReadOnlyCollection<DeviceSessionCollection> sessions, DeviceUpdatePolicyWrapper updatePolicyWrapper)
        {
            _updatePolicyWrapper = updatePolicyWrapper;
            _sessionsByAddress = sessions.ToDictionary(sessionsByDevice => sessionsByDevice.Device.Address, 
                sessionByDevice => sessionByDevice);
        }

        public DeviceSessionCollection GetNextDevice()
        {
            ForeignDevice device = _updatePolicyWrapper.GetNextDevice();
            return _sessionsByAddress[device.Address];
        }

        public void Dispose()
        {
            foreach (DeviceSessionCollection deviceSessionCollection in _sessionsByAddress.Values)
                deviceSessionCollection.Dispose();
        }
    }
}