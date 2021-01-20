using System;
using System.Collections.Generic;
using System.Threading;

namespace Nandaka.Core.Device
{
    internal sealed class MasterDeviceDispatcher
    {
        private readonly IDeviceUpdatePolicy _updatePolicy;

        public IReadOnlyCollection<ForeignDeviceCtx> SlaveDevices { get; }

        public TimeSpan RequestTimeout => _updatePolicy.RequestTimeout;
        
        public MasterDeviceDispatcher(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, IDeviceUpdatePolicy updatePolicy)
        {
            SlaveDevices = slaveDevices;
            _updatePolicy = updatePolicy;
        }

        public ForeignDeviceCtx GetNextDevice()
        {
            ForeignDeviceCtx nextDeviceCtx = _updatePolicy.GetNextDevice(SlaveDevices, out bool isUpdateCycleCompleted);
            
            if (isUpdateCycleCompleted)
                Thread.Sleep(_updatePolicy.UpdateTimeout);

            return nextDeviceCtx;
        }

        public void OnMessageReceived(ForeignDeviceCtx deviceCtx)
        {
            _updatePolicy.OnMessageReceived(deviceCtx);
        }

        public void OnErrorOccured(ForeignDeviceCtx deviceCtx, DeviceError error)
        {
            _updatePolicy.OnErrorOccured(deviceCtx, error);
        }

        public void OnUnexpectedDeviceResponse(ForeignDeviceCtx expectedDeviceCtx, int responseDeviceAddress)
        {
            _updatePolicy.OnUnexpectedDeviceResponse(SlaveDevices, expectedDeviceCtx, responseDeviceAddress);
        }
    }
}