using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicy
    {
        TimeSpan RequestTimeout { get; }
        TimeSpan UpdateTimeout { get; }
        NandakaDevice GetNextDevice(IReadOnlyCollection<NandakaDevice> slaveDevices, ILog log, out bool isUpdateCycleCompleted);
        void OnMessageReceived(NandakaDevice device, ILog log);
        void OnErrorOccured(NandakaDevice device, DeviceError error, ILog log);
        void OnUnexpectedDeviceResponse(IReadOnlyCollection<NandakaDevice> slaveDevices, NandakaDevice expectedDevice, int responseDeviceAddress, ILog log);
    }
}
