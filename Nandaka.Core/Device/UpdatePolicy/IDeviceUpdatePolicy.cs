using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan RequestTimeout { get; }
        TimeSpan UpdateTimeout { get; }
        ForeignDevice GetNextDevice(IReadOnlyCollection<ForeignDevice> slaveDevices, out bool isUpdateCycleCompleted);
        void OnMessageReceived(ForeignDevice device);
        void OnErrorOccured(ForeignDevice device, DeviceError error);
        void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDevice> slaveDevices, ForeignDevice expectedDevice, int responseDeviceAddress);
    }
}
