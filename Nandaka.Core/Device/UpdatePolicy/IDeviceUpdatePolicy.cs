using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan RequestTimeout { get; }
        TimeSpan UpdateTimeout { get; }
        ForeignDeviceCtx GetNextDevice(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, out bool isUpdateCycleCompleted);
        void OnMessageReceived(ForeignDeviceCtx deviceCtx);
        void OnErrorOccured(ForeignDeviceCtx deviceCtx, DeviceError error);
        void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, ForeignDeviceCtx expectedDeviceCtx, int responseDeviceAddress);
    }
}
