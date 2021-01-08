using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan RequestTimeout { get; }
        TimeSpan UpdateTimeout { get; }
        ForeignDeviceCtx GetNextDevice(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, ILog log, out bool isUpdateCycleCompleted);
        void OnMessageReceived(ForeignDeviceCtx deviceCtx, ILog log);
        void OnErrorOccured(ForeignDeviceCtx deviceCtx, DeviceError error, ILog log);
        void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, ForeignDeviceCtx expectedDeviceCtx, int responseDeviceAddress, ILog log);
    }
}
