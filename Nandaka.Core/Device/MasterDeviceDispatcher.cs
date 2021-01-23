using System;
using System.Collections.Generic;
using System.Threading;

namespace Nandaka.Core.Device
{
    internal sealed class MasterDeviceDispatcher
    {
        private readonly IDeviceUpdatePolicy _updatePolicy;

        public IReadOnlyCollection<ForeignDevice> SlaveDevices { get; }

        public TimeSpan RequestTimeout => _updatePolicy.RequestTimeout;
        
        public MasterDeviceDispatcher(IReadOnlyCollection<ForeignDevice> slaveDevices, IDeviceUpdatePolicy updatePolicy)
        {
            SlaveDevices = slaveDevices;
            _updatePolicy = updatePolicy;
        }

        public ForeignDevice GetNextDevice()
        {
            ForeignDevice nextDevice = _updatePolicy.GetNextDevice(SlaveDevices, out bool isUpdateCycleCompleted);
            
            if (isUpdateCycleCompleted)
                Thread.Sleep(_updatePolicy.UpdateTimeout);

            return nextDevice;
        }

        public void OnMessageReceived(ForeignDevice device)
        {
            _updatePolicy.OnMessageReceived(device);
        }

        public void OnErrorOccured(ForeignDevice device, DeviceError error)
        {
            _updatePolicy.OnErrorOccured(device, error);
        }

        public void OnUnexpectedDeviceResponse(ForeignDevice expectedDevice, int responseDeviceAddress)
        {
            _updatePolicy.OnUnexpectedDeviceResponse(SlaveDevices, expectedDevice, responseDeviceAddress);
        }
    }
}